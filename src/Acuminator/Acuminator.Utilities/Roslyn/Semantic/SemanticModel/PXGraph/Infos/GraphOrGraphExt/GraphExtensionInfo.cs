using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared;

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

		public ExtensionMechanismType BaseExtensionsMechanismType { get; }

		/// <summary>
		/// The overridden base graph extensions if any.<br/>
		/// Contains either direct base graph extension or chained graph extensions.
		/// </summary>
		public ImmutableArray<GraphExtensionInfo> BaseGraphExtensions { get; }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder) :
							 this(node, graphExtension, graph, declarationOrder, baseGraphExtensions: [],
								  ExtensionMechanismType.None)
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, GraphExtensionInfo baseGraphExtension,
									ExtensionMechanismType extensionMechanismType) :
							 this(node, graphExtension, graph, declarationOrder, [baseGraphExtension.CheckIfNull()], 
								  extensionMechanismType)
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, IEnumerable<GraphExtensionInfo> baseGraphExtensions,
									ExtensionMechanismType extensionMechanismType) :
							 this(node, graphExtension, graph, declarationOrder, baseGraphExtensions?.ToImmutableArray() ?? [],
								  extensionMechanismType)
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, ImmutableArray<GraphExtensionInfo> baseGraphExtensions,
									ExtensionMechanismType extensionMechanismType) :
							 base(node, graphExtension, declarationOrder)
		{
			BaseGraph 					= graph;
			BaseExtensionsMechanismType = extensionMechanismType;
			BaseGraphExtensions 		= baseGraphExtensions;

			CombineWithBaseGraphAndGraphExtensions();
		}

		public override IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromDerivedExtensionToBaseGraph(bool includeSelf)
		{
			var graphInfos = BaseGraph?.GetInfosFromDerivedExtensionToBaseGraph(includeSelf);
			var extensionInfos = GetExtensionInfosFromDerivedExtensionToBaseExtensions(includeSelf);

			return graphInfos != null
				? extensionInfos.Concat(graphInfos)
				: extensionInfos;
		}

		public override IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromBaseGraphToDerivedExtension(bool includeSelf)
		{
			var graphInfos = BaseGraph?.GetInfosFromBaseGraphToDerivedExtension(includeSelf);
			var extensionInfos = GetInfosFromBaseGraphToDerivedExtension(includeSelf);

			return graphInfos != null
				? graphInfos.Concat(extensionInfos)
				: extensionInfos;
		}

		/// <summary>
		/// Gets extension infos from base extensions to derived extension level by level.
		/// </summary>
		/// <param name="includeSelf">True to include self, false to exclude.</param>
		/// <returns>
		/// Collection of extension infos from base extensions to derived extension level by level.
		/// </returns>
		public IEnumerable<GraphExtensionInfo> GetExtensionInfosFromBaseExtensionsToDerivedExtension(bool includeSelf) =>
			GetExtensionInfosFromDerivedExtensionToBaseExtensions(includeSelf).Reverse();

		/// <summary>
		/// Get extension infos from derived extension to base extensions level by level.
		/// </summary>
		/// <param name="includeSelf">True to include self, false to exclude.</param>
		/// <returns>
		/// Collection of extension infos from derived extension to base extensions level by level.
		/// </returns>
		public IEnumerable<GraphExtensionInfo> GetExtensionInfosFromDerivedExtensionToBaseExtensions(bool includeSelf)
		{
			if (includeSelf)
				yield return this;

			if (BaseGraphExtensions.IsDefaultOrEmpty)
				yield break;

			// Use breadth first traversal to get level by level extensions + add hard guard on iterations count against infinite loops
			const int maxIterationCount = 10_000;
			int iterationCount = 0;
			var queue = new Queue<GraphExtensionInfo>(BaseGraphExtensions);

			while (queue.Count > 0 && iterationCount < maxIterationCount)
			{
				iterationCount++;
				var baseOrChainedGraphExtension = queue.Dequeue();
				yield return baseOrChainedGraphExtension;

				if (baseOrChainedGraphExtension.BaseGraphExtensions.IsDefaultOrEmpty)
					continue;

				foreach (var descendantGraphExtension in baseOrChainedGraphExtension.BaseGraphExtensions)
				{
					queue.Enqueue(descendantGraphExtension);
				}
			}
		}

		/// <summary>
		/// Combine this info with info from base graph and graph extensions.
		/// </summary>
		private void CombineWithBaseGraphAndGraphExtensions()
		{

		}
	}
}