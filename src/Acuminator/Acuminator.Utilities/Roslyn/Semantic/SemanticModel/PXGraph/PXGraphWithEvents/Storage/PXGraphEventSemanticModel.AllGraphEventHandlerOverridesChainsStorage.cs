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
		/// <inheritdoc cref="IGraphEventHandlerOverridesChainsStorage"/>
		private class AllGraphEventHandlerOverridesChainsStorage : IGraphEventHandlerOverridesChainsStorage
		{
			public int AllEventHandlersCount { get; }

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowSelectingEventHandlers => RowSelectingByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowSelectedEventHandlers => RowSelectedByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowInsertingEventHandlers => RowInsertingByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowInsertedEventHandlers => RowInsertedByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEventHandlers => RowUpdatingByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEventHandlers => RowUpdatedByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowDeletingEventHandlers => RowDeletingByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowDeletedEventHandlers => RowDeletedByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowPersistingEventHandlers => RowPersistingByName.Values;

			public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowPersistedEventHandlers => RowPersistedByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldSelectingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEventHandlers => FieldSelectingByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldDefaultingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEventHandlers => FieldDefaultingByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldVerifyingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEventHandlers => FieldVerifyingByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEventHandlers => FieldUpdatingByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatedByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEventHandlers => FieldUpdatedByName.Values;

			public ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> CacheAttachedByName { get; }
			public IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEventHandlers => CacheAttachedByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> CommandPreparingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEventHandlers => CommandPreparingByName.Values;

			public ImmutableDictionary<string, GraphFieldEventHandlerInfo> ExceptionHandlingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEventHandlers => ExceptionHandlingByName.Values;

			public AllGraphEventHandlerOverridesChainsStorage(ImmutableDictionary<string, GraphRowEventHandlerInfo> rowSelectingByName, 
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowSelectedByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowInsertingByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowInsertedByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowUpdatingByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowUpdatedByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowDeletingByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowDeletedByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowPersistingByName,
											 ImmutableDictionary<string, GraphRowEventHandlerInfo> rowPersistedByName,

											 ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> cacheAttachedByName,

											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldSelectingByName,
											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldDefaultingByName,
											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldVerifyingByName,
											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldUpdatingByName,
											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldUpdatedByName,

											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> commandPreparingByName,
											 ImmutableDictionary<string, GraphFieldEventHandlerInfo> exceptionHandlingByName)
			{
				RowSelectingByName = rowSelectingByName;
				RowSelectedByName = rowSelectedByName;

				RowInsertingByName = rowInsertingByName;
				RowInsertedByName = rowInsertedByName;

				RowUpdatingByName = rowUpdatingByName;
				RowUpdatedByName = rowUpdatedByName;

				RowDeletingByName = rowDeletingByName;
				RowDeletedByName = rowDeletedByName;

				RowPersistingByName = rowPersistingByName;
				RowPersistedByName = rowPersistedByName;

				CacheAttachedByName = cacheAttachedByName;

				FieldSelectingByName  = fieldSelectingByName;
				FieldDefaultingByName = fieldDefaultingByName;
				FieldVerifyingByName  = fieldVerifyingByName;
				FieldUpdatingByName   = fieldUpdatingByName;
				FieldUpdatedByName 	  = fieldUpdatedByName;

				CommandPreparingByName = commandPreparingByName;
				ExceptionHandlingByName = exceptionHandlingByName;

				AllEventHandlersCount = RowSelectingByName.Count   + RowSelectedByName.Count	 + RowInsertingByName.Count		+ RowInsertedByName.Count +
										RowUpdatingByName.Count    + RowUpdatedByName.Count		 + RowDeletingByName.Count		+ RowDeletedByName.Count +
										RowPersistingByName.Count  + RowPersistedByName.Count    + CacheAttachedByName.Count	+
										FieldSelectingByName.Count + FieldDefaultingByName.Count + FieldVerifyingByName.Count   +
										FieldUpdatingByName.Count  + FieldUpdatedByName.Count	 + CommandPreparingByName.Count + ExceptionHandlingByName.Count;
			}

			public static IGraphEventHandlerOverridesChainsStorage FromCollected(NaturalOrderEventHandlersCollector eventsCollector) =>
				new AllGraphEventHandlerOverridesChainsStorage(
					 rowSelectingByName: GetRowEventHandlers(eventsCollector, EventType.RowSelecting),
		 			 rowSelectedByName: GetRowEventHandlers(eventsCollector, EventType.RowSelected),

			 		 rowInsertingByName: GetRowEventHandlers(eventsCollector, EventType.RowInserting),
		 			 rowInsertedByName: GetRowEventHandlers(eventsCollector, EventType.RowInserted),

		 			 rowUpdatingByName: GetRowEventHandlers(eventsCollector, EventType.RowUpdating),
					 rowUpdatedByName: GetRowEventHandlers(eventsCollector, EventType.RowUpdated),

		 			 rowDeletingByName: GetRowEventHandlers(eventsCollector, EventType.RowDeleting),
					 rowDeletedByName: GetRowEventHandlers(eventsCollector, EventType.RowDeleted),

				 	 rowPersistingByName: GetRowEventHandlers(eventsCollector, EventType.RowPersisting),
			 		 rowPersistedByName: GetRowEventHandlers(eventsCollector, EventType.RowPersisted),

				 	 cacheAttachedByName: GetCacheAttachedEventHandlers(eventsCollector),

					 fieldSelectingByName: GetFieldEventHandlers(eventsCollector, EventType.FieldSelecting),
					 fieldDefaultingByName: GetFieldEventHandlers(eventsCollector, EventType.FieldDefaulting),
					 fieldVerifyingByName: GetFieldEventHandlers(eventsCollector, EventType.FieldVerifying),
				 	 fieldUpdatingByName: GetFieldEventHandlers(eventsCollector, EventType.FieldUpdating),
			 		 fieldUpdatedByName: GetFieldEventHandlers(eventsCollector, EventType.FieldUpdated),

					 commandPreparingByName: GetFieldEventHandlers(eventsCollector, EventType.CommandPreparing),
					 exceptionHandlingByName: GetFieldEventHandlers(eventsCollector, EventType.ExceptionHandling)
					);

			public IEnumerable<GraphEventHandlerInfoBase> GetEventHandlersByEventType(EventType eventType) => eventType switch
			{
				EventType.RowSelecting 	=> RowSelectingEventHandlers,
				EventType.RowSelected 	=> RowSelectedEventHandlers,
				EventType.RowInserting 	=> RowInsertingEventHandlers,
				EventType.RowInserted 	=> RowInsertedEventHandlers,
				EventType.RowUpdating 	=> RowUpdatingEventHandlers,
				EventType.RowUpdated 	=> RowUpdatedEventHandlers,
				EventType.RowDeleting 	=> RowDeletingEventHandlers,
				EventType.RowDeleted 	=> RowDeletedEventHandlers,
				EventType.RowPersisting => RowPersistingEventHandlers,
				EventType.RowPersisted 	=> RowPersistedEventHandlers,

				EventType.FieldSelecting  => FieldSelectingEventHandlers,
				EventType.FieldDefaulting => FieldDefaultingEventHandlers,
				EventType.FieldVerifying  => FieldVerifyingEventHandlers,
				EventType.FieldUpdating   => FieldUpdatingEventHandlers,
				EventType.FieldUpdated 	  => FieldUpdatedEventHandlers,

				EventType.CacheAttached 	=> CacheAttachedEventHandlers,
				EventType.CommandPreparing 	=> CommandPreparingEventHandlers,
				EventType.ExceptionHandling => ExceptionHandlingEventHandlers,
				EventType.None 				=> [],
				_ 							=> throw new NotSupportedException($"Event type {eventType} is not supported")
			};

			public IEnumerable<GraphEventHandlerInfoBase> GetAllEventHandlers()
			{
				IEnumerable<GraphEventHandlerInfoBase> allEventHandlers = RowSelectingByName.Values;

				AppendEventHandlers(RowSelectedByName);
				AppendEventHandlers(RowInsertingByName);
				AppendEventHandlers(RowInsertedByName);
				AppendEventHandlers(RowUpdatingByName);
				AppendEventHandlers(RowUpdatedByName);
				AppendEventHandlers(RowDeletingByName);
				AppendEventHandlers(RowDeletedByName);
				AppendEventHandlers(RowPersistingByName);
				AppendEventHandlers(RowPersistedByName);

				AppendEventHandlers(FieldSelectingByName);
				AppendEventHandlers(FieldDefaultingByName);
				AppendEventHandlers(FieldVerifyingByName);
				AppendEventHandlers(FieldUpdatingByName);
				AppendEventHandlers(FieldUpdatedByName);

				AppendEventHandlers(CacheAttachedByName);

				AppendEventHandlers(CommandPreparingByName);
				AppendEventHandlers(ExceptionHandlingByName);

				return allEventHandlers;

				//------------------------------------Local Function----------------------------------------------
				void AppendEventHandlers<THandler>(ImmutableDictionary<string, THandler> eventHandlersByName)
				where THandler : GraphEventHandlerInfoBase<THandler>
				{
					if (eventHandlersByName.Count > 0)
						allEventHandlers = allEventHandlers.Concat(eventHandlersByName.Values);
				}
			}

			private static ImmutableDictionary<string, GraphRowEventHandlerInfo> GetRowEventHandlers(NaturalOrderEventHandlersCollector eventsCollector, EventType eventType)
			{
				OverridableItemsCollection<GraphRowEventHandlerInfo>? rawCollection = eventsCollector.GetRowEventHandlers(eventType);
				return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphRowEventHandlerInfo>.Empty;
			}

			private static ImmutableDictionary<string, GraphFieldEventHandlerInfo> GetFieldEventHandlers(NaturalOrderEventHandlersCollector eventsCollector, EventType eventType)
			{
				OverridableItemsCollection<GraphFieldEventHandlerInfo>? rawCollection = eventsCollector.GetFieldEventHandlers(eventType);
				return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphFieldEventHandlerInfo>.Empty;
			}

			private static ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> GetCacheAttachedEventHandlers(NaturalOrderEventHandlersCollector eventsCollector)
			{
				OverridableItemsCollection<GraphCacheAttachedEventHandlerInfo>? rawCollection = eventsCollector.CacheAttachedEventHandlers;
				return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo>.Empty;
			}
		}
	}
}
