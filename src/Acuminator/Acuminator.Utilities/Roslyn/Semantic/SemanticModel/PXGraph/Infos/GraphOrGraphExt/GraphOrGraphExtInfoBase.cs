using System;

using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo

{
	protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol graphOrGraphExt, int declarationOrder) :
								 base(node, graphOrGraphExt, declarationOrder)
	{ }
}