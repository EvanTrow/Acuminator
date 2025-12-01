using System;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphInfo : GraphOrGraphExtInfoBase, IOverridableItem<GraphInfo>
	{
		public GraphInfo? Base { get; }

		internal GraphInfo(ClassDeclarationSyntax? node, ITypeSymbol graph, int declarationOrder, GraphInfo baseGraphInfo) :
					  base(node, graph, declarationOrder)
		{
			Base = baseGraphInfo.CheckIfNull();
			CombineWithBaseInfo();
		}

		internal GraphInfo(ClassDeclarationSyntax? node, ITypeSymbol graph, int declarationOrder) :
					  base(node, graph, declarationOrder)
		{ }

		public static GraphInfo? Create(ITypeSymbol? graph, ClassDeclarationSyntax? graphNode, PXContext pxContext,
										int graphDeclarationOrder, CancellationToken cancellation)
		{
			if (graph == null || !graph.IsPXGraph(pxContext))
				return null;

			return CreateUnsafe(graph, graphNode, pxContext, graphDeclarationOrder, cancellation);
		}

		internal static GraphInfo CreateUnsafe(ITypeSymbol graph, ClassDeclarationSyntax? graphNode, PXContext pxContext,
											   int graphDeclarationOrder, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var graphBaseTypesFromBaseToDerived = graph.GetGraphBaseTypes()
													   .Reverse();
			bool isInSource = graphNode != null;
			GraphInfo? aggregatedBaseGraphInfo = null, prevGraphInfo = null;

			foreach (ITypeSymbol baseType in graphBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseGraphNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseGraphNode != null;
				aggregatedBaseGraphInfo = prevGraphInfo != null
					? new GraphInfo(baseGraphNode, baseType, declarationOrder: 0, prevGraphInfo)
					: new GraphInfo(baseGraphNode, baseType, declarationOrder: 0);

				prevGraphInfo = aggregatedBaseGraphInfo;
			}

			var graphInfo = aggregatedBaseGraphInfo != null
				? new GraphInfo(graphNode, graph, graphDeclarationOrder, aggregatedBaseGraphInfo)
				: new GraphInfo(graphNode, graph, graphDeclarationOrder);

			return graphInfo;
		}

		void IOverridableItem<GraphInfo>.CombineWithBaseInfo() => CombineWithBaseInfo();

		private void CombineWithBaseInfo()
		{
		}
	}
}