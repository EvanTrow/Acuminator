using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the correctly declared graph extension without issues in the type hierarchy.
	/// </summary>
	public class GraphExtensionInfo : GraphOrGraphExtInfoBase
	{
		public GraphInfo? BaseGraph { get; }

		/// <summary>
		/// The overridden base graph extensions if any.<br/>
		/// Contains either direct base graph extension or chained graph extensions.
		/// </summary>
		ImmutableArray<GraphExtensionInfo> BaseGraphExtensions { get; }

		public bool IsFirstLevelExtension => BaseGraphExtensions.IsDefaultOrEmpty;

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder) :
							 this(node, graphExtension, graph, declarationOrder, baseGraphExtensions: [])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, GraphExtensionInfo baseGraphExtension) :
							 this(node, graphExtension, graph, declarationOrder,
								  [baseGraphExtension.CheckIfNull()])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, IEnumerable<GraphExtensionInfo> baseGraphExtensions) :
							 this(node, graphExtension, graph, declarationOrder, 
								  baseGraphExtensions?.ToImmutableArray() ?? [])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, ImmutableArray<GraphExtensionInfo> baseGraphExtensions) :
							 base(node, graphExtension, declarationOrder)
		{
			BaseGraph = graph;
			BaseGraphExtensions = baseGraphExtensions;
			CombineWithBaseGraphAndGraphExtensions();
		}

		/// <summary>
		/// Combine this info with info from base graph and graph extensions.
		/// </summary>
		private void CombineWithBaseGraphAndGraphExtensions()
		{

		}

		public static (GraphExtensionInfo? Info, bool HasCircularReferences) Create(ITypeSymbol? graphExtension, 
																				ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol? graph,
																				PXContext pxContext, int graphExtDeclarationOrder, CancellationToken cancellation)
		{
			var graphNode = graph.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var graphInfo = GraphInfo.Create(graph, graphNode, pxContext, graphDeclarationOrder: 0, cancellation);

			return Create(graphExtension, graphExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder, cancellation);
		}

		public static (GraphExtensionInfo? Info, bool HasCircularReferences) Create(ITypeSymbol? graphExtension, 
																				ClassDeclarationSyntax? graphExtensionNode, GraphInfo? graphInfo,
																				PXContext pxContext, int graphExtDeclarationOrder, CancellationToken cancellation)
		{
			var visitedExtensions = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
			return Create(graphExtension, graphExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder, visitedExtensions, cancellation);
		}

		private static (GraphExtensionInfo? Info, bool HasCircularReferences) Create(ITypeSymbol? graphExtension, 
																				ClassDeclarationSyntax? graphExtensionNode, GraphInfo? graphInfo,
																				PXContext pxContext, int graphExtDeclarationOrder, 
																				HashSet<ITypeSymbol> visitedExtensions, 
																				CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (graphExtension == null)
				return (Info: null, HasCircularReferences: false);
			else if (!visitedExtensions.Add(graphExtension))
				return (Info: null, HasCircularReferences: true);

			ITypeSymbol? extensionBaseType = graphExtension.GetBaseTypesAndThis()
																.FirstOrDefault(type => type.IsGraphExtensionBaseType());
			if (extensionBaseType == null)
				return (Info: null, HasCircularReferences: false);
			
			bool isInSource = graphExtensionNode != null;
			var (extensionFromPreviousLevels, hasCircularReferences) = GetAggregatedExtensionFromPreviousLevels(extensionBaseType, pxContext, 
																												 visitedExtensions, graphInfo, 
																												 cancellation);
			if (hasCircularReferences)
				return (Info: null, HasCircularReferences: true);

			GraphExtensionInfo? aggregatedBaseGraphExtInfo = null;
			
			if (!SymbolEqualityComparer.Default.Equals(graphExtension.BaseType, extensionBaseType))
			{
				(aggregatedBaseGraphExtInfo, hasCircularReferences) = 
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

		private static (GraphExtensionInfo? Info, bool HasCircularReferences) GetAggregatedExtensionFromPreviousLevels(
																					ITypeSymbol extensionBaseType, PXContext pxContext,
																					HashSet<ITypeSymbol> collectedExtensionsFromOtherLevels,
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

		private static (GraphExtensionInfo? Info, bool HasCircularReferences) GetAggregatedBaseExtensions(ITypeSymbol graphExtension, 
																					GraphInfo? graphInfo, GraphExtensionInfo? aggregatedExtensionFromBaseLevels,
																					HashSet<ITypeSymbol> collectedExtensionsFromOtherLevels, bool isInSource, 
																					CancellationToken cancellation)
		{
			var graphExtensionsBaseTypesFromBaseToDerived = graphExtension.GetGraphExtensionBaseTypes()
																		  .Reverse();

			GraphExtensionInfo? aggregatedBaseGraphExtensionInfo = null; 
			GraphExtensionInfo? prevGraphExtensionInfo = aggregatedExtensionFromBaseLevels;

			foreach (ITypeSymbol baseType in graphExtensionsBaseTypesFromBaseToDerived)
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