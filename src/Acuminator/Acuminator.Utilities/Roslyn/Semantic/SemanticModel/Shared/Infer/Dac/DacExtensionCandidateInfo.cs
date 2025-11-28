using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer
{
	public class DacExtensionCandidateInfo : ExtensionCandidateInfo<DacInfo, DacExtensionInfo>
	{
		public DacExtensionCandidateInfo(ClassDeclarationSyntax? extensionNode, INamedTypeSymbol extensionSymbol, int declarationOrder) :
									base(extensionNode, extensionSymbol, declarationOrder)
		{
		}

		public override DacExtensionInfo? GetFrameworkTypeInfo()
		{
			if (HasCircularReferences || HasMultipleRootTypes)
				return null;

			var baseDacInfo = RootTypes.FirstOrDefault();
			var baseDacExtensionInfo = BaseExtensions.FirstOrDefault();

			// Create the DAC extension info from the candidate info
			return baseDacExtensionInfo != null
				? new DacExtensionInfo(Node, Symbol, baseDacInfo, DeclarationOrder, baseDacExtensionInfo)
				: new DacExtensionInfo(Node, Symbol, baseDacInfo, DeclarationOrder);
		}
	}
}