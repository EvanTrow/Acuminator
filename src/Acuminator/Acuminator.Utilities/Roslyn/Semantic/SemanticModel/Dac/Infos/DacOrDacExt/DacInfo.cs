using System;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacInfo : DacOrDacExtInfoBase<DacInfo>
	{
		internal DacInfo(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, DacInfo baseInfo) :
					base(node, dac, declarationOrder, baseInfo)
		{
		}

		internal DacInfo(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
					base(node, dac, declarationOrder)
		{
		}
	}
}