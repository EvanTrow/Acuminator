using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphInfo : GraphOrGraphExtInfoBase, IOverridableItem<GraphInfo>
	{
		public GraphInfo? Base { get; }

		public override ITypeSymbol GraphType => Symbol;

		internal GraphInfo(ClassDeclarationSyntax? node, ITypeSymbol graph, int declarationOrder, GraphInfo baseGraphInfo) :
					  base(node, graph, declarationOrder)
		{
			Base = baseGraphInfo.CheckIfNull();
			CombineWithBaseInfo();
		}

		internal GraphInfo(ClassDeclarationSyntax? node, ITypeSymbol graph, int declarationOrder) :
					  base(node, graph, declarationOrder)
		{ }

		public override IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromDerivedExtensionToBaseGraph(bool includeSelf) =>
			includeSelf
				? this.ThisAndOverriddenItems()
				: this.JustOverriddenItems();

		public override IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromBaseGraphToDerivedExtension(bool includeSelf) =>
			GetInfosFromDerivedExtensionToBaseGraph(includeSelf).Reverse();

		void IOverridableItem<GraphInfo>.CombineWithBaseInfo() => CombineWithBaseInfo();

		private void CombineWithBaseInfo()
		{
		}
	}
}