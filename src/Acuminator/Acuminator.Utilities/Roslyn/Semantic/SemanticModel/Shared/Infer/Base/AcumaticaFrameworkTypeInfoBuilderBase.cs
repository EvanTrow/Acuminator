using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer
{
	public abstract class AcumaticaFrameworkTypeInfoBuilderBase<TRootInfo, TExtensionInfo>
	where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
	where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
	{
		public virtual IInferredAcumaticaFrameworkTypeInfo? InferTypeInfo(INamedTypeSymbol? typeSymbol, PXContext pxContext,
																		  int? customDeclarationOrder, CancellationToken cancellation)
		{
			pxContext.ThrowOnNull();

			if (typeSymbol == null)
				return null;

			cancellation.ThrowIfCancellationRequested();

			if (IsRootFrameworkType(typeSymbol, pxContext))
			{
				var rootSymbolInfo = CreateRootSymbolInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);
				return rootSymbolInfo;
			}
			else if (IsExtensionType(typeSymbol, pxContext))
			{
				//var ownExtensionGraph = GetRootTypeFromExtensionType(typeSymbol, pxContext);//typeSymbol.GetGraphFromGraphExtension(pxContext);

				//if (ownExtensionGraph == null)
				//	return null;



				//var graphNode = ownExtensionGraph.GetSyntax(cancellation) as ClassDeclarationSyntax;
				//var graphInfo = GraphInfo.Create(graph as INamedTypeSymbol, graphNode, pxContext, graphDeclarationOrder: 0, cancellation);

				var extensionSymbolInfo = CreateExtensionSymbolInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);
				return extensionSymbolInfo;
			}
			else
				return null;
		}

		/// <summary>
		/// Determines whether the <paramref name="typeSymbol"/> is a root Acumatica Framework type (for example, graph or DAC).
		/// </summary>
		/// <param name="typeSymbol">The type symbol.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// <see langword="true"/> if the <paramref name="typeSymbol"/> is a root Acumatica type; otherwise, <see langword="false"/>.
		/// </returns>
		protected abstract bool IsRootFrameworkType(INamedTypeSymbol typeSymbol, PXContext pxContext);

		/// <summary>
		/// Determines whether the <paramref name="typeSymbol"/> is an extension type (for example, graph extension or DAC extension).
		/// </summary>
		/// <param name="typeSymbol">The type symbol.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// <see langword="true"/> if the <paramref name="typeSymbol"/> is an extension type; otherwise, <see langword="false"/>.
		/// </returns>
		protected abstract bool IsExtensionType(INamedTypeSymbol typeSymbol, PXContext pxContext);

		/// <summary>
		/// Creates the root symbol information.
		/// </summary>
		/// <param name="rootTypeSymbol">The root type symbol.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="customDeclarationOrder">The custom declaration order.</param>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// The created root symbol information.
		/// </returns>
		protected virtual TRootInfo? CreateRootSymbolInfo(INamedTypeSymbol rootTypeSymbol, PXContext pxContext,
														  int? customDeclarationOrder, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = rootTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var rootBaseTypesFromBaseToDerived = GetBaseRootTypes(rootTypeSymbol).OfType<INamedTypeSymbol>()
																				 .Reverse();
			bool isInSource = rootNode != null;
			TRootInfo? aggregatedBaseRootTypesInfo = null, prevRootInfo = null;

			foreach (INamedTypeSymbol baseType in rootBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseRootNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseRootNode != null;
				aggregatedBaseRootTypesInfo = prevRootInfo != null
					? RootSymbolInfoConstructorWithBaseInfo(baseRootNode, baseType, declarationOrder: 0, prevRootInfo)
					: RootSymbolInfoConstructor(baseRootNode, baseType, declarationOrder: 0);

				prevRootInfo = aggregatedBaseRootTypesInfo;
			}

			int rootDeclarationOrder = customDeclarationOrder ?? 0;
			var rootInfo = aggregatedBaseRootTypesInfo != null
				? RootSymbolInfoConstructorWithBaseInfo(rootNode, rootTypeSymbol, rootDeclarationOrder, aggregatedBaseRootTypesInfo)
				: RootSymbolInfoConstructor(rootNode, rootTypeSymbol, rootDeclarationOrder);

			return rootInfo;
		}

		protected abstract IEnumerable<ITypeSymbol> GetBaseRootTypes(INamedTypeSymbol baseRootTypeSymbol);

		protected abstract TRootInfo RootSymbolInfoConstructor(ClassDeclarationSyntax? node, INamedTypeSymbol rootType,
															   int declarationOrder);

		protected abstract TRootInfo RootSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? node, INamedTypeSymbol rootType, 
																		   int declarationOrder, TRootInfo baseRootInfo);

		/// <summary>
		/// Gets the root type from the extension type <paramref name="extensionTypeSymbol"/>.
		/// </summary>
		/// <param name="extensionTypeSymbol">The extension type symbol.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <returns>
		/// The root type from the extension type.
		/// </returns>
		protected abstract ITypeSymbol? GetRootTypeFromExtensionType(INamedTypeSymbol extensionTypeSymbol, PXContext pxContext);

		

		

		protected virtual TExtensionInfo? CreateExtensionSymbolInfo(INamedTypeSymbol? graphExtension, PXContext pxContext, int? customDeclarationOrder, 
																	CancellationToken cancellation)
		{
			var visitedExtensions = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

			cancellation.ThrowIfCancellationRequested();

			if (graphExtension == null)
				return (Info: null, HasCircularReferences: false);
			else if (!visitedExtensions.Add(graphExtension))
				return (Info: null, HasCircularReferences: true);

			INamedTypeSymbol? extensionBaseType = graphExtension.GetBaseTypesAndThis()
																.FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
			if (extensionBaseType == null)
				return (Info: null, HasCircularReferences: false);

			bool isInSource = graphExtensionNode != null;
			var (extensionFromPreviousLevels, hasCurcularReferences) = GetAggregatedExtensionFromPreviouslLevels(extensionBaseType, pxContext,
																												 visitedExtensions, graphInfo,
																												 cancellation);
			if (hasCurcularReferences)
				return (Info: null, HasCircularReferences: true);

			GraphExtensionInfo? aggregatedBaseGraphExtInfo = null;

			if (!SymbolEqualityComparer.Default.Equals(graphExtension.BaseType, extensionBaseType))
			{
				(aggregatedBaseGraphExtInfo, hasCurcularReferences) =
					GetAggregatedBaseExtensions(graphExtension, graphInfo, extensionFromPreviousLevels, visitedExtensions, isInSource, cancellation);
			}

			GraphOrGraphExtInfoBase? baseInfo = aggregatedBaseGraphExtInfo ?? extensionFromPreviousLevels ?? graphInfo as GraphOrGraphExtInfoBase;
			var graphExtensionInfo = baseInfo switch
			{
				GraphInfo baseGraphInfo => new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo,
																  graphExtDeclarationOrder, baseGraphInfo),

				GraphExtensionInfo baseExtensionInfo => new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo,
																			   graphExtDeclarationOrder, baseExtensionInfo),

				_ => new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, graphExtDeclarationOrder)
			};

			return (graphExtensionInfo, HasCircularReferences: false);
		}

		private static (GraphExtensionInfo? Info, bool HasCircularReferences) GetAggregatedExtensionFromPreviouslLevels(
																					INamedTypeSymbol extensionBaseType, PXContext pxContext,
																					HashSet<INamedTypeSymbol> collectedExtensionsFromOtherLevels,
																					GraphInfo? graphInfo, CancellationToken cancellation)
		{
			if (!extensionBaseType.IsGenericType)
				return (Info: null, HasCircularReferences: false);

			var typeArguments = extensionBaseType.TypeArguments;

			if (typeArguments.Length <= 1)
				return (Info: null, HasCircularReferences: false);

			if (typeArguments[0] is not INamedTypeSymbol previousLevelExtensionType || !previousLevelExtensionType.IsPXGraphExtension(pxContext))
				return (Info: null, HasCircularReferences: false);

			if (collectedExtensionsFromOtherLevels.Contains(previousLevelExtensionType))
				return (Info: null, HasCircularReferences: true);

			var prevLevelExtensionNode = previousLevelExtensionType.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var aggregatedPrevLevelGraphExtensionInfo =
				Create(previousLevelExtensionType, prevLevelExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder: 0, collectedExtensionsFromOtherLevels, cancellation);

			return aggregatedPrevLevelGraphExtensionInfo;
		}

		private static (GraphExtensionInfo? Info, bool HasCircularReferences) GetAggregatedBaseExtensions(INamedTypeSymbol graphExtension,
																					GraphInfo? graphInfo, GraphExtensionInfo? aggregatedExtensionFromBaseLevels,
																					HashSet<INamedTypeSymbol> collectedExtensionsFromOtherLevels, bool isInSource,
																					CancellationToken cancellation)
		{
			var graphExtensionsBaseTypesFromBaseToDerived = graphExtension.GetGraphExtensionBaseTypes()
																		  .OfType<INamedTypeSymbol>()
																		  .Reverse();

			GraphExtensionInfo? aggregatedBaseGraphExtensionInfo = null;
			GraphExtensionInfo? prevGraphExtensionInfo = aggregatedExtensionFromBaseLevels;

			foreach (INamedTypeSymbol baseType in graphExtensionsBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				if (collectedExtensionsFromOtherLevels.Contains(baseType))
					return (Info: null, HasCircularReferences: true);

				var baseGraphExtensionNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseGraphExtensionNode != null;
				aggregatedBaseGraphExtensionInfo = prevGraphExtensionInfo != null
					? new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, declarationOrder: 0, prevGraphExtensionInfo)
					: graphInfo != null
						? new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, declarationOrder: 0, graphInfo)
						: new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, declarationOrder: 0);

				prevGraphExtensionInfo = aggregatedBaseGraphExtensionInfo;
			}

			return (aggregatedBaseGraphExtensionInfo, HasCircularReferences: false);
		}
	}
}