using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Base interface for graph event handlers storage.
	/// </summary>
	public interface IGraphEventHandlersStorage
	{
		IEnumerable<GraphEventHandlerInfoBase> GetEventHandlersByEventType(EventType eventType);

		IEnumerable<GraphEventHandlerInfoBase> GetAllEventHandlers();

		int AllEventHandlersCount { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowSelectingEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowSelectedEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowInsertingEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowInsertedEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowDeletingEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowDeletedEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowPersistingEventHandlers { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowPersistedEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEventHandlers { get; }

		IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEventHandlers { get; }

		IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEventHandlers { get; }
	}
}
