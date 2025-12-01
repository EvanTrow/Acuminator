using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer
{
	public abstract class ExtensionCandidateInfo<TRootInfo, TExtensionInfo> : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>,
																			  IInferredAcumaticaFrameworkTypeInfo,
																			  IHaveDeclarationOrder
	where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
	where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
	{
		protected internal List<TExtensionInfo> BaseExtensionsMutable { get; } =  new(capacity: 1);

		public IReadOnlyCollection<TExtensionInfo> BaseExtensions => BaseExtensionsMutable;

		protected internal List<TRootInfo> RootTypesMutable { get; } = new(capacity: 1);

		public IReadOnlyCollection<TRootInfo> RootTypes => RootTypesMutable;

		public ITypeSymbol? CircularReferenceExtension { get; internal set; }

		public bool HasCircularReferences => CircularReferenceExtension != null;

		public bool HasMultipleRootTypes => RootTypes.Count > 1;

		protected ExtensionCandidateInfo(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol, int declarationOrder) :
									base(extensionNode, extensionSymbol, declarationOrder)
		{
		}

		public abstract TExtensionInfo? GetFrameworkTypeInfo();
	}
}