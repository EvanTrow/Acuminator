using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class AsyncOperationsSymbols : SymbolsSetBase
	{
		public ImmutableArray<IMethodSymbol> StartOperation_PXLongOperation { get; }

		public ImmutableArray<IMethodSymbol> AsyncOperationsFromILongOperationManager { get; }

		public INamedTypeSymbol PXLongOperation { get; }

		public INamedTypeSymbol? ILongOperationManager { get; }

		/// <summary>
		/// All methods that can start async long running operations.
		/// </summary>
		public ImmutableHashSet<IMethodSymbol> StartOperation_AllMethods { get; }

		internal AsyncOperationsSymbols(Compilation compilation) : base(compilation)
		{
			PXLongOperation = Compilation.GetTypeByMetadataName(TypeFullNames.Async.PXLongOperation)!;
			ILongOperationManager = Compilation.GetTypeByMetadataName(TypeFullNames.Async.ILongOperationManager) ??
									Compilation.GetTypeByMetadataName(TypeFullNames.Async.ILongOperationManagerOld);

			var allStartOperationMethods = PXLongOperation.GetMethods(DelegateNames.Async.StartOperation)
														  .ToList(capacity: 16);
			StartOperation_PXLongOperation = allStartOperationMethods.ToImmutableArray();

			if (ILongOperationManager != null)
			{
				var startOperationFromILongOperationManager		 = ILongOperationManager.GetMethods(DelegateNames.Async.StartOperation);
				var startAsyncOperationFromILongOperationManager = ILongOperationManager.GetMethods(DelegateNames.Async.StartAsyncOperation);
				var awaitFromILongOperationManager				 = ILongOperationManager.GetMethods(DelegateNames.Async.Await);

				AsyncOperationsFromILongOperationManager = startOperationFromILongOperationManager
																.Concat(startAsyncOperationFromILongOperationManager)
																.Concat(awaitFromILongOperationManager)
																.ToImmutableArray();

				allStartOperationMethods.AddRange(AsyncOperationsFromILongOperationManager);
			}
			else
			{
				AsyncOperationsFromILongOperationManager = [];
			}

			StartOperation_AllMethods = allStartOperationMethods.ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
		}
	}
}
