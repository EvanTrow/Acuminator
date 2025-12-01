using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer
{
	public class GraphExtensionCandidateInfo : ExtensionCandidateInfo<GraphInfo, GraphExtensionInfo>
	{
		public GraphExtensionCandidateInfo(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol, int declarationOrder) :
									  base(extensionNode, extensionSymbol, declarationOrder)
		{
		}

		public override GraphExtensionInfo? GetFrameworkTypeInfo()
		{
			if (HasCircularReferences || HasMultipleRootTypes)
				return null;

			var baseGraphInfo = RootTypes.FirstOrDefault();

			// Create the graph extension info from the candidate info
			return new GraphExtensionInfo(Node, Symbol, baseGraphInfo, DeclarationOrder, BaseExtensions);
		}
	}
}