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

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public abstract class AcumaticaFrameworkTypeInfoBuilderBase<TRootInfo, TExtensionInfo>
where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
{
	public virtual IInferredAcumaticaFrameworkTypeInfo? InferTypeInfo(ITypeSymbol? typeSymbol, PXContext pxContext,
																	  int? customDeclarationOrder, CancellationToken cancellation)
	{
		pxContext.ThrowOnNull();

		if (typeSymbol?.BaseType == null)
			return null;

		cancellation.ThrowIfCancellationRequested();

		if (IsRootFrameworkType(typeSymbol, pxContext))
		{
			var rootSymbolInfo = CreateRootSymbolInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);
			return rootSymbolInfo;
		}
		else if (IsExtensionType(typeSymbol, pxContext))
		{
			var inferredInfo = InferExtensionInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);

			if (inferredInfo == null)
				return null;

			return inferredInfo.Value.InferredExtensionInfo as IInferredAcumaticaFrameworkTypeInfo ?? 
				   inferredInfo.Value.ExtensionInfoCandidate;
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
	protected abstract bool IsRootFrameworkType(ITypeSymbol typeSymbol, PXContext pxContext);

	/// <summary>
	/// Determines whether the <paramref name="typeSymbol"/> is an extension type (for example, graph extension or DAC extension).
	/// </summary>
	/// <param name="typeSymbol">The type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// <see langword="true"/> if the <paramref name="typeSymbol"/> is an extension type; otherwise, <see langword="false"/>.
	/// </returns>
	protected abstract bool IsExtensionType(ITypeSymbol typeSymbol, PXContext pxContext);

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
	protected virtual TRootInfo? CreateRootSymbolInfo(ITypeSymbol rootTypeSymbol, PXContext pxContext,
													  int? customDeclarationOrder, CancellationToken cancellation)
	{
		cancellation.ThrowIfCancellationRequested();

		var rootNode = rootTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;
		var rootBaseTypesFromBaseToDerived = GetBaseRootTypes(rootTypeSymbol).Reverse();
		bool isInSource = rootNode != null;
		TRootInfo? aggregatedBaseRootTypesInfo = null, prevRootInfo = null;

		foreach (ITypeSymbol baseType in rootBaseTypesFromBaseToDerived)
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

	/// <summary>
	/// Gets the base root types of the <paramref name="rootTypeSymbol"/>.
	/// </summary>
	/// <param name="rootTypeSymbol">The root type symbol.</param>
	/// <returns>
	/// Base root types of the <paramref name="rootTypeSymbol"/>.
	/// </returns>
	protected abstract IEnumerable<ITypeSymbol> GetBaseRootTypes(ITypeSymbol rootTypeSymbol);

	protected abstract TRootInfo RootSymbolInfoConstructor(ClassDeclarationSyntax? node, ITypeSymbol rootType,
														   int declarationOrder);

	protected abstract TRootInfo RootSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? node, ITypeSymbol rootType, 
																	   int declarationOrder, TRootInfo baseRootInfo);

	/// <summary>
	/// Gets the root type from the extension type <paramref name="extensionTypeSymbol"/>.
	/// </summary>
	/// <param name="extensionTypeSymbol">The extension type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The root type from the extension type.
	/// </returns>
	protected abstract ITypeSymbol? GetRootTypeFromExtensionType(ITypeSymbol extensionTypeSymbol, PXContext pxContext);

	protected virtual InferredExtensionSymbolInfo<TRootInfo, TExtensionInfo>? InferExtensionInfo(ITypeSymbol extensionTypeSymbol,
																								 PXContext pxContext, int? customDeclarationOrder, 
																								 CancellationToken cancellation)
	{
		cancellation.ThrowIfCancellationRequested();
		var extensionNode = extensionTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;

		// Trivial popular hot path optimization
		if (extensionTypeSymbol.BaseType.IsGraphExtensionBaseType())
		{
			var baseRootTypeSymbol = GetRootTypeFromExtensionType(extensionTypeSymbol, pxContext);

			if (baseRootTypeSymbol == null)
				return null;

			var rootSymbolInfo = CreateRootSymbolInfo(baseRootTypeSymbol, pxContext, customDeclarationOrder: null, cancellation);
			int extensionDeclarationOrder = customDeclarationOrder ?? 0;
			var extensionInfo = ExtensionSymbolInfoConstructor(extensionNode, extensionTypeSymbol, rootSymbolInfo, extensionDeclarationOrder);
			return new InferredExtensionSymbolInfo<TRootInfo, TExtensionInfo>(extensionInfo);
		}

		//, GraphInfo? graphInfo,
		//var ownExtensionGraph = GetRootTypeFromExtensionType(typeSymbol, pxContext);//typeSymbol.GetGraphFromGraphExtension(pxContext);

		//if (ownExtensionGraph == null)
		//	return null;



		//var graphNode = ownExtensionGraph.GetSyntax(cancellation) as ClassDeclarationSyntax;
		//var graphInfo = GraphInfo.Create(graph as INamedTypeSymbol, graphNode, pxContext, graphDeclarationOrder: 0, cancellation);

		var visitedExtensions = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		return Create(graphExtension, graphExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder, visitedExtensions, cancellation);
	}

	protected ExtensionCandidateInfo<TRootInfo, TExtensionInfo> InferExtensionCandidate(INamedTypeSymbol? extensionTypeSymbol,
																			ClassDeclarationSyntax? extensionNode, PXContext pxContext, 
																			int extensionDeclarationOrder, HashSet<INamedTypeSymbol> visitedExtensions,
																			int recursionDepth, CancellationToken cancellation)
	{
		cancellation.ThrowIfCancellationRequested();

		if (extensionTypeSymbol == null)
			return (Info: null, HasCircularReferences: false);
		else if (!visitedExtensions.Add(extensionTypeSymbol))
			return (Info: null, HasCircularReferences: true);

		INamedTypeSymbol? extensionBaseType = extensionTypeSymbol.GetBaseTypesAndThis()
															.FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
		if (extensionBaseType == null)
			return (Info: null, HasCircularReferences: false);

		bool isInSource = extensionNode != null;
		var (extensionFromPreviousLevels, hasCircularReferences) = GetAggregatedExtensionFromPreviousLevels(extensionBaseType, pxContext,
																											 visitedExtensions, graphInfo,
																											 cancellation);
		if (hasCircularReferences)
			return (Info: null, HasCircularReferences: true);

		GraphExtensionInfo? aggregatedBaseGraphExtInfo = null;

		if (!SymbolEqualityComparer.Default.Equals(extensionTypeSymbol.BaseType, extensionBaseType))
		{
			(aggregatedBaseGraphExtInfo, hasCircularReferences) =
				GetAggregatedBaseExtensions(extensionTypeSymbol, graphInfo, extensionFromPreviousLevels, visitedExtensions, isInSource, cancellation);
		}

		GraphOrGraphExtInfoBase? baseInfo = aggregatedBaseGraphExtInfo ?? extensionFromPreviousLevels ?? graphInfo as GraphOrGraphExtInfoBase;
		var graphExtensionInfo = baseInfo switch
		{
			GraphInfo baseGraphInfo => new GraphExtensionInfo(extensionNode, extensionTypeSymbol, graphInfo,
															  extensionDeclarationOrder, baseGraphInfo),

			GraphExtensionInfo baseExtensionInfo => new GraphExtensionInfo(extensionNode, extensionTypeSymbol, graphInfo,
																		   extensionDeclarationOrder, baseExtensionInfo),

			_ => new GraphExtensionInfo(extensionNode, extensionTypeSymbol, graphInfo, extensionDeclarationOrder)
		};

		return (graphExtensionInfo, HasCircularReferences: false);
	}

	private static (GraphExtensionInfo? Info, bool HasCircularReferences) GetAggregatedExtensionFromPreviousLevels(
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


	protected abstract TExtensionInfo ExtensionSymbolInfoConstructor(ClassDeclarationSyntax? node, ITypeSymbol rootType, TRootInfo? rootInfo,
																	 int declarationOrder);
}