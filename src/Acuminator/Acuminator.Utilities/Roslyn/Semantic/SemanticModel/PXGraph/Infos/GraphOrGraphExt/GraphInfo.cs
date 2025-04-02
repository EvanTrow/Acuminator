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
		public new GraphInfo? Base => base.Base as GraphInfo;

		protected GraphInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graph, int declarationOrder, GraphInfo baseInfo) :
					   base(node, graph, declarationOrder, baseInfo)
		{ }

		protected GraphInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graph, int declarationOrder) :
					   base(node, graph, declarationOrder)
		{ }

		protected sealed override void CombineWithBaseInfo(GraphOrGraphExtInfoBase baseInfo) 
		{
			if (baseInfo is GraphInfo graphInfo)
				CombineWithBaseInfo(graphInfo);
		}

		protected virtual void CombineWithBaseInfo(GraphInfo baseInfo)
		{
		}

		public static GraphInfo? Create(INamedTypeSymbol? graph, ClassDeclarationSyntax? graphNode, PXContext pxContext,
										int graphDeclarationOrder, CancellationToken cancellation)
		{
			if (graph == null)
				return null;

			cancellation.ThrowIfCancellationRequested();

			var graphBaseTypesFromBaseToDerived = graph.GetGraphBaseTypes()
													   .OfType<INamedTypeSymbol>()
													   .Reverse();
			bool isInSource = graphNode != null;
			GraphInfo? aggregatedBaseGraphInfo = null, prevGraphInfo = null;

			foreach (INamedTypeSymbol baseType in graphBaseTypesFromBaseToDerived)
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
	}
}