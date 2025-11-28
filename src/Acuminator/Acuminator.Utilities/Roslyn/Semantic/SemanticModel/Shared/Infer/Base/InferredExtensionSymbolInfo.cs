using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public record struct InferredExtensionSymbolInfo<TRootInfo, TExtensionInfo>
where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
{
	public TExtensionInfo? InferredExtensionInfo { get; }

	public ExtensionCandidateInfo<TRootInfo, TExtensionInfo>? ExtensionInfoCandidate { get; }

	public InferredExtensionSymbolInfo(TExtensionInfo inferredExtensionInfo)
	{
		InferredExtensionInfo  = inferredExtensionInfo.CheckIfNull();
		ExtensionInfoCandidate = null;
	}

	public InferredExtensionSymbolInfo(ExtensionCandidateInfo<TRootInfo, TExtensionInfo> extensionInfoCandidate)
	{
		InferredExtensionInfo = null;
		ExtensionInfoCandidate = extensionInfoCandidate.CheckIfNull();
	}
}