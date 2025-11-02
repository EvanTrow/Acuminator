using System;
using System.Collections.Immutable;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class AsyncOperationsSymbols : SymbolsSetBase
	{
		public INamedTypeSymbol PXLongOperation { get; }

		public ImmutableArray<IMethodSymbol> StartOperation_PXLongOperation { get; }

		public ImmutableArray<IMethodSymbol> StartOperation_AllMethods { get; }

		internal AsyncOperationsSymbols(Compilation compilation) : base(compilation)
		{
			PXLongOperation = Compilation.GetTypeByMetadataName(TypeFullNames.Async.PXLongOperation)!;
			StartOperation_PXLongOperation = PXLongOperation.GetMethods(DelegateNames.StartOperation)
															.ToImmutableArray();
			StartOperation_AllMethods = StartOperation_PXLongOperation;
		}
	}
}
