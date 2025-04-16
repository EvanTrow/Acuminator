using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Storage of graph event handlers overrides chains.
	/// </summary>
	/// <remarks>
	/// Unlike the <see cref="IDeclaredGraphEventHandlerStorage"/>, for a particular event type, DAC and optionally DAC field<br/>
	/// this storage keeps not an array of declared event handlers, but the top of the event handler's overrides chain.
	/// </remarks>
	public interface IGraphEventHandlerOverridesChainsStorage : IGraphEventHandlersStorage
	{
		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectingByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectedByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertingByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertedByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatingByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatedByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletingByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletedByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistingByName { get; }

		ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistedByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldSelectingByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldDefaultingByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldVerifyingByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatingByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatedByName { get; }

		ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> CacheAttachedByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> CommandPreparingByName { get; }

		ImmutableDictionary<string, GraphFieldEventHandlerInfo> ExceptionHandlingByName { get; }
	}
}
