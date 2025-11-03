using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXCacheSymbols : SymbolsSetForTypeBase
	{
		/// <summary>
		/// All overloads of <c>PXCache.Insert</c> and <c>PXCache&lt;TNode&gt;.Insert</c> methods
		/// </summary>
		public ImmutableArray<IMethodSymbol> Insert { get; }

		/// <summary>
		/// All overloads of <c>PXCache.Update</c> and <c>PXCache&lt;TNode&gt;.Update</c> methods
		/// </summary>
		public ImmutableArray<IMethodSymbol> Update { get; }

		/// <summary>
		/// All overloads of <c>PXCache.Delete</c> and <c>PXCache&lt;TNode&gt;.Delete</c> methods
		/// </summary>
		public ImmutableArray<IMethodSymbol> Delete { get; }

		/// <summary>
		/// All overloads of <c>PXCache.RaiseExceptionHandling</c> and <c>PXCache&lt;TNode&gt;.RaiseExceptionHandling</c> methods
		/// </summary>
		public ImmutableArray<IMethodSymbol> RaiseExceptionHandling { get; }

		/// <summary>
		/// The generic <c>PXCache&lt;TNode&gt;</c> type
		/// </summary>
		public INamedTypeSymbol GenericType { get; }
		
		public IEventSymbol? RowSelectingWhileReading { get; }

		internal PXCacheSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXCache)
		{
			Type.ThrowOnNull();

			GenericType = Compilation.GetTypeByMetadataName(TypeFullNames.PXCache1)!;

			Insert = GetPXCacheMethodOverloads(DelegateNames.Insert);
			Update = GetPXCacheMethodOverloads(DelegateNames.Update);
			Delete = GetPXCacheMethodOverloads(DelegateNames.Delete);

			RaiseExceptionHandling   = GetPXCacheMethodOverloads(DelegateNames.RaiseExceptionHandling);
			RowSelectingWhileReading = Type.GetMembers(Events.Names.PXCache.RowSelectingWhileReading)
										   .OfType<IEventSymbol>()
										   .FirstOrDefault();
		}

		private ImmutableArray<IMethodSymbol> GetPXCacheMethodOverloads(string methodName)
		{
			var nonGenericPXCacheMethods = Type.GetMethods(methodName);
			var genericPXCacheMethods	 = GenericType.GetMethods(methodName)
													  .Where(m => !m.IsOverride && m.IsDeclaredInType(GenericType));
			return nonGenericPXCacheMethods.Concat(genericPXCacheMethods)
										   .ToImmutableArray();
		}
	}
}