using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the action delegate in graph or graph extension.
	/// </summary>
	public sealed class ActionDelegateInfo : OverridableNodeSymbolItem<ActionDelegateInfo, MethodDeclarationSyntax, IMethodSymbol>
	{
		public ActionDelegateInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder) :
							 base(node, symbol, declarationOrder)
		{
		}

		public ActionDelegateInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder, ActionDelegateInfo baseInfo) :
							 base(node, symbol, declarationOrder, baseInfo)
		{
			CombineWithBaseInfo();
		}

		protected override void CombineWithBaseInfo() { }
	}
}
