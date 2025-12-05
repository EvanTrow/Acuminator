using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public record InferredSymbolInfo
{
	public IInferredAcumaticaSymbolInfo InferredInfo { get; }

	public IReadOnlyCollection<ITypeSymbol> CollectedRootTypes { get; }

	public ITypeSymbol? CircularReferenceExtension { get; init; }

	public ITypeSymbol? ExtensionWithBadBaseExtensions { get; init; }

	public bool FailedToCollectTypeHierarchy { get; init; }

	public InferredSymbolInfo(IInferredAcumaticaSymbolInfo inferredInfo, IReadOnlyCollection<ITypeSymbol>? collectedRootTypes)
	{
		InferredInfo = inferredInfo.CheckIfNull();
		CollectedRootTypes = collectedRootTypes ?? Array.Empty<ITypeSymbol>();
	}

	public InferResultKind GetResultKind()
	{
		if (CircularReferenceExtension != null)
			return InferResultKind.CircularReferences;
		else if (CollectedRootTypes.Count > 1)
			return InferResultKind.MultipleRootTypes;
		else if (ExtensionWithBadBaseExtensions != null)
			return InferResultKind.BadBaseExtensions;
		else if (FailedToCollectTypeHierarchy)
			return InferResultKind.UnrecognizedError;
		else
			return InferResultKind.Success;
	}
}