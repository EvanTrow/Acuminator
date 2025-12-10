using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public record InferredSymbolInfo
{
	public IInferredAcumaticaSymbolInfo? InferredInfo { get; }

	public ITypeSymbol? CircularReferenceExtension { get; init; }

	public ITypeSymbol? ExtensionWithBadBaseExtensions { get; init; }

	public bool FailedToCollectTypeHierarchy { get; init; }

	public InferredSymbolInfo(IInferredAcumaticaSymbolInfo? inferredInfo)
	{
		InferredInfo = inferredInfo;
	}

	public InferResultKind GetResultKind()
	{
		if (CircularReferenceExtension != null)
			return InferResultKind.CircularReferences;
		else if (ExtensionWithBadBaseExtensions != null)
			return InferResultKind.BadBaseExtensions;
		else if (FailedToCollectTypeHierarchy)
			return InferResultKind.UnrecognizedError;
		else
			return InferResultKind.Success;
	}
}