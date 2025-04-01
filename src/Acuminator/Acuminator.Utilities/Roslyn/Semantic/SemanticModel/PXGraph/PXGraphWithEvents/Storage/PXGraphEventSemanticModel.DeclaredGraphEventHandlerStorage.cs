using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel
	{
		/// <inheritdoc cref="IDeclaredGraphEventHandlerStorage"/>
		private class DeclaredGraphEventHandlerStorage : IDeclaredGraphEventHandlerStorage
		{
			public int AllEventHandlersCount { get; }

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowSelectingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowSelectingEventHandlers => RowSelectingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowSelectedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowSelectedEventHandlers => RowSelectedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowInsertingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowInsertingEventHandlers => RowInsertingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowInsertedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowInsertedEventHandlers => RowInsertedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowUpdatingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEventHandlers => RowUpdatingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowUpdatedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEventHandlers => RowUpdatedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowDeletingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowDeletingEventHandlers => RowDeletingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowDeletedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowDeletedEventHandlers => RowDeletedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowPersistingByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowPersistingEventHandlers => RowPersistingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> RowPersistedByName { get; }
			public IEnumerable<GraphRowEventHandlerInfo> RowPersistedEventHandlers => RowPersistedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldSelectingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEventHandlers => FieldSelectingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldDefaultingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEventHandlers => FieldDefaultingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldVerifyingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEventHandlers => FieldVerifyingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldUpdatingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEventHandlers => FieldUpdatingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> FieldUpdatedByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEventHandlers => FieldUpdatedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphCacheAttachedEventHandlerInfo>> CacheAttachedByName { get; }
			public IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEventHandlers => CacheAttachedByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> CommandPreparingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEventHandlers => CommandPreparingByName.Values.SelectMany(handler => handler);

			public ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> ExceptionHandlingByName { get; }
			public IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEventHandlers => ExceptionHandlingByName.Values.SelectMany(handler => handler);

			public DeclaredGraphEventHandlerStorage(ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowSelectingByName, 
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowSelectedByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowInsertingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowInsertedByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowUpdatingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowUpdatedByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowDeletingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowDeletedByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowPersistingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphRowEventHandlerInfo>> rowPersistedByName,

													 ImmutableDictionary<string, ImmutableArray<GraphCacheAttachedEventHandlerInfo>> cacheAttachedByName,

													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> fieldSelectingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> fieldDefaultingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> fieldVerifyingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> fieldUpdatingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> fieldUpdatedByName,

													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> commandPreparingByName,
													 ImmutableDictionary<string, ImmutableArray<GraphFieldEventHandlerInfo>> exceptionHandlingByName)
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

				FieldSelectingByName = fieldSelectingByName;
				FieldDefaultingByName = fieldDefaultingByName;
				FieldVerifyingByName = fieldVerifyingByName;
				FieldUpdatingByName = fieldUpdatingByName;
				FieldUpdatedByName = fieldUpdatedByName;

				CommandPreparingByName = commandPreparingByName;
				ExceptionHandlingByName = exceptionHandlingByName;

				AllEventHandlersCount = Count(RowSelectingByName)   + Count(RowSelectedByName)	   + Count(RowInsertingByName)	   + Count(RowInsertedByName) +
										Count(RowUpdatingByName)    + Count(RowUpdatedByName)	   + Count(RowDeletingByName)	   + Count(RowDeletedByName)  +
										Count(RowPersistingByName)  + Count(RowPersistedByName)    + Count(CacheAttachedByName)    +
										Count(FieldSelectingByName) + Count(FieldDefaultingByName) + Count(FieldVerifyingByName)   +
										Count(FieldUpdatingByName)  + Count(FieldUpdatedByName)    + Count(CommandPreparingByName) + Count(ExceptionHandlingByName);
			}

			private static int Count<THandler>(ImmutableDictionary<string, ImmutableArray<THandler>> handlersByName) =>
				handlersByName.Count > 0 
					? handlersByName.Values.Sum(handlers => handlers.Length)
					: 0;

			public static IDeclaredGraphEventHandlerStorage FromOverridesChainsStorageInNaturalCollectionOrder(INamedTypeSymbol containingType,
																				IGraphEventHandlerOverridesChainsStorage overrideChainsStorage) =>
				new DeclaredGraphEventHandlerStorage(
						 rowSelectingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowSelectingByName),
						 rowSelectedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowSelectedByName),

						 rowInsertingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowInsertingByName),
						 rowInsertedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowInsertedByName),

						 rowUpdatingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowUpdatingByName),
						 rowUpdatedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowUpdatedByName),

						 rowDeletingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowDeletingByName),
						 rowDeletedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowDeletedByName),

						 rowPersistingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowPersistingByName),
						 rowPersistedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.RowPersistedByName),

						 cacheAttachedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.CacheAttachedByName),

						 fieldSelectingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.FieldSelectingByName),
						 fieldDefaultingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.FieldDefaultingByName),
						 fieldVerifyingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.FieldVerifyingByName),
						 fieldUpdatingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.FieldUpdatingByName),
						 fieldUpdatedByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.FieldUpdatedByName),

						 commandPreparingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.CommandPreparingByName),
						 exceptionHandlingByName: GetDeclaredEventHandlersInNaturalOrder(containingType, overrideChainsStorage.ExceptionHandlingByName)
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
				if (AllEventHandlersCount == 0)
					return [];

				IEnumerable<GraphEventHandlerInfoBase> allEventHandlers = RowSelectingByName.Count > 0 
					? RowSelectingByName.Values.SelectMany(handler => handler)
					: [];

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
				void AppendEventHandlers<THandler>(ImmutableDictionary<string, ImmutableArray<THandler>> eventHandlersByName)
				where THandler : GraphEventHandlerInfoBase<THandler>
				{
					if (eventHandlersByName.Count > 0)
						allEventHandlers = allEventHandlers.Concat(eventHandlersByName.Values.SelectMany(handler => handler));
				}
			}

			private static ImmutableDictionary<string, ImmutableArray<THandler>> GetDeclaredEventHandlersInNaturalOrder<THandler>(INamedTypeSymbol containingType,
																								ImmutableDictionary<string, THandler> overridesChainsOfSameEventType)
			where THandler : GraphEventHandlerInfoBase<THandler>
			{
				if (overridesChainsOfSameEventType.Count == 0)
					return ImmutableDictionary<string, ImmutableArray<THandler>>.Empty;

				Dictionary<string, ImmutableArray<THandler>>? declaredEventHandlers = null;

				foreach (var (key, mostDerivedEventHandler) in overridesChainsOfSameEventType)
				{
					var handlersArray = GetDeclaredEventHandlersFromOverridesChain(containingType, mostDerivedEventHandler);

					if (handlersArray != null)
					{
						declaredEventHandlers ??= new Dictionary<string, ImmutableArray<THandler>>(overridesChainsOfSameEventType.Count, 
																								   overridesChainsOfSameEventType.KeyComparer);
						declaredEventHandlers.Add(key, handlersArray.Value);
					}
				}

				return declaredEventHandlers?.ToImmutableDictionary(keyComparer: overridesChainsOfSameEventType.KeyComparer) ??
					   ImmutableDictionary<string, ImmutableArray<THandler>>.Empty;
			}

			private static ImmutableArray<THandler>? GetDeclaredEventHandlersFromOverridesChain<THandler>(INamedTypeSymbol containingType, 
																										  THandler mostDerivedEventHandler)
			where THandler : GraphEventHandlerInfoBase<THandler>
			{
				if (!mostDerivedEventHandler.Symbol.IsDeclaredInType(containingType))
					return null;

				var handlersArrayBuilder = ImmutableArray.CreateBuilder<THandler>(initialCapacity: 1);
				handlersArrayBuilder.Add(mostDerivedEventHandler);

				if (mostDerivedEventHandler.Base == null)     // optimization for most popular case when handler is not an override
					return handlersArrayBuilder.ToImmutable();

				var baseHandlersDeclaredInType = mostDerivedEventHandler.JustOverridenItems()
																		.TakeWhile(handler => handler.Symbol.IsDeclaredInType(containingType));
				handlersArrayBuilder.AddRange(baseHandlersDeclaredInType);
				return handlersArrayBuilder.ToImmutable();
			}
		}
	}
}
