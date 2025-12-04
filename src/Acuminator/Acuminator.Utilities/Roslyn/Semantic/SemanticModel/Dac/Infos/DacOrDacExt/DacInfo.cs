using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public sealed class DacInfo : DacOrDacExtInfoBase<DacInfo>
	{
		internal DacInfo(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, DacInfo baseInfo) :
					base(node, dac, declarationOrder, baseInfo)
		{
			CombineWithBaseInfo();
		}

		internal DacInfo(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
					base(node, dac, declarationOrder)
		{
		}

		public override IEnumerable<DacOrDacExtInfoBase> GetInfosFromDerivedExtensionToBaseDac(bool includeSelf) =>
			includeSelf
				? this.ThisAndOverriddenItems()
				: this.JustOverriddenItems();

		protected override void CombineWithBaseInfo() { }
	}
}