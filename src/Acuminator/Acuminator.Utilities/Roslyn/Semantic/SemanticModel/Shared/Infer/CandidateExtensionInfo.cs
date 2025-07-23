using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class CandidateExtensionInfo<TBaseGraphOrDacInfo> : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IHaveDeclarationOrder
	where TBaseGraphOrDacInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>
	{
		protected readonly List<TBaseGraphOrDacInfo> _baseGraphOrDacs = new(capacity: 1);

		public IReadOnlyCollection<TBaseGraphOrDacInfo> BaseGraphsOrDacs => _baseGraphOrDacs;

		protected readonly List<CandidateExtensionInfo<TBaseGraphOrDacInfo>> _baseExtensions = new(capacity: 1);

		public IReadOnlyCollection<CandidateExtensionInfo<TBaseGraphOrDacInfo>> BaseExtensions => _baseExtensions;

		public bool HasCircularReferences { get; private set; }

		protected CandidateExtensionInfo(ClassDeclarationSyntax? extensionNode, INamedTypeSymbol extensionSymbol, int declarationOrder) :
									base(extensionNode, extensionSymbol, declarationOrder)
		{
			
		}
	}
}