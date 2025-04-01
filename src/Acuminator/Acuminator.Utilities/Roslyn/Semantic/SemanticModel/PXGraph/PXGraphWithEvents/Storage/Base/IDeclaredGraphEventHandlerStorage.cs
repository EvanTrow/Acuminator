using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Storage of graph event handlers declared in type.
	/// </summary>
	/// <remarks>
	/// Unlike the <see cref="IGraphEventHandlerOverridesChainsStorage"/> this storage contains not a top of the override chain, but an array of declared event handlers.<br/>
	/// The array of event handlers for a particular event type, DAC and optionally DAC field is sorted by the declaration order descending, <br/>
	/// and the first element in the array is the most derived event handler.
	/// </remarks>
	public interface IDeclaredGraphEventHandlerStorage : IGraphEventHandlersStorage
	{
		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowSelectingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowSelectedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowInsertingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowInsertedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowUpdatingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowUpdatedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowDeletingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowDeletedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowPersistingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowPersistedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldSelectingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldDefaultingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldVerifyingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldUpdatingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldUpdatedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphCacheAttachedEventHandlerInfo>> CacheAttachedByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> CommandPreparingByName { get; }

		ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> ExceptionHandlingByName { get; }
	}
}
