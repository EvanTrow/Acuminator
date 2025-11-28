using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer
{
	public abstract class ExtensionCandidateInfo<TAcumaticaFrameworkType, TExtensionInfo> : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, 
																							IInferredAcumaticaFrameworkTypeInfo<TExtensionInfo>,
																							IHaveDeclarationOrder
	where TAcumaticaFrameworkType : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>
	where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>
	{
		protected internal List<TExtensionInfo> BaseExtensionsMutable { get; } =  new(capacity: 1);

		public IReadOnlyCollection<TExtensionInfo> BaseExtensions => BaseExtensionsMutable;

		protected internal List<TAcumaticaFrameworkType> RootTypesMutable { get; } = new(capacity: 1);

		public IReadOnlyCollection<TAcumaticaFrameworkType> RootTypes => RootTypesMutable;

		public bool HasCircularReferences 
		{ 
			get;
			internal set;
		}

		public bool HasMultipleRootTypes => RootTypes.Count > 1;

		protected ExtensionCandidateInfo(ClassDeclarationSyntax? extensionNode, INamedTypeSymbol extensionSymbol, int declarationOrder) :
									base(extensionNode, extensionSymbol, declarationOrder)
		{
		}

		public abstract TExtensionInfo? GetFrameworkTypeInfo();
	}
}