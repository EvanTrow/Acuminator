using System;

using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo

{
	/// <inheritdoc path="/summary"/>
	/// <remarks>
	/// <inheritdoc path="/remarks"/>
	/// <br/>
	/// The constructed correct graph and graph extension info can't have circular references.
	/// </remarks>
	bool IInferredAcumaticaFrameworkTypeInfo.HasCircularReferences => false;

	/// <inheritdoc path="/summary"/>
	/// <remarks>
	/// <inheritdoc path="/remarks"/>
	/// <br/><br/>
	/// The constructed correct graph and graph extension info can't have multiple root types.
	/// </remarks>
	bool IInferredAcumaticaFrameworkTypeInfo.HasMultipleRootTypes => false;

	/// <inheritdoc path="/summary"/>
	/// <remarks>
	/// <inheritdoc path="/remarks"/>
	/// <br/><br/>
	/// Graph and constructed graph extension info have successfully collected type hierarchy.
	/// </remarks>
	bool IInferredAcumaticaFrameworkTypeInfo.FailedToCollectTypeHierarchy => false;

	protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol graphOrGraphExt, int declarationOrder) :
								 base(node, graphOrGraphExt, declarationOrder)
	{ }
}