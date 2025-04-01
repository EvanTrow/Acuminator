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

		IEnumerable<GraphRowEventHandlerInfo> RowSelectingEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowSelectedEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowInsertingEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowInsertedEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowDeletingEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowDeletedEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowPersistingEvents { get; }

		IEnumerable<GraphRowEventHandlerInfo> RowPersistedEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEvents { get; }

		IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEvents { get; }

		IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEvents { get; }
	}
}
