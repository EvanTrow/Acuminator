using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel
	{
		/// <summary>
		/// Interface for graph event handlers storage.
		/// </summary>
		public interface IGraphEventHandlersStorage
		{
			IEnumerable<GraphEventHandlerInfoBase> GetEventHandlersByEventType(EventType eventType);

			IEnumerable<GraphEventHandlerInfoBase> GetAllEventHandlers();

			int AllEventHandlersCount { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectingByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowSelectingEvents { get; }


			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectedByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowSelectedEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertingByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowInsertingEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertedByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowInsertedEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatingByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatedByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletingByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowDeletingEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletedByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowDeletedEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistingByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowPersistingEvents { get; }

			ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistedByName { get; }
			IEnumerable<GraphRowEventHandlerInfo> RowPersistedEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldSelectingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldDefaultingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldVerifyingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatedByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEvents { get; }

			ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> CacheAttachedByName { get; }
			IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> CommandPreparingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEvents { get; }

			ImmutableDictionary<string, GraphFieldEventHandlerInfo> ExceptionHandlingByName { get; }
			IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEvents { get; }
		}
	}
}
