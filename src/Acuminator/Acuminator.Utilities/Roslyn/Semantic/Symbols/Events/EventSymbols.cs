using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class EventSymbols : SymbolsSetBase
	{
		internal EventSymbols(Compilation compilation) : base(compilation)
		{
			_eventTypeMap = new Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>>(
				() => CreateEventTypeMap(this));
			_eventHandlerSignatureTypeMap = new Lazy<IReadOnlyDictionary<EventHandlerGeneralInfo, INamedTypeSymbol>>(
				() => CreateEventHandlerSignatureTypeMap(this));
		}

		private readonly Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>> _eventTypeMap;
		public IReadOnlyDictionary<ITypeSymbol, EventType> EventTypeMap => _eventTypeMap.Value;

		private readonly Lazy<IReadOnlyDictionary<EventHandlerGeneralInfo, INamedTypeSymbol>> _eventHandlerSignatureTypeMap;
		public IReadOnlyDictionary<EventHandlerGeneralInfo, INamedTypeSymbol> EventHandlerSignatureTypeMap => _eventHandlerSignatureTypeMap.Value;

		public INamedTypeSymbol PXRowSelectingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowSelectingEventArgs)!;
		public INamedTypeSymbol PXRowSelectedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowSelectedEventArgs)!;
		public INamedTypeSymbol PXRowInsertingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowInsertingEventArgs)!;
		public INamedTypeSymbol PXRowInsertedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowInsertedEventArgs)!;
		public INamedTypeSymbol PXRowUpdatingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowUpdatingEventArgs)!;
		public INamedTypeSymbol PXRowUpdatedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowUpdatedEventArgs)!;
		public INamedTypeSymbol PXRowDeletingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowDeletingEventArgs)!;
		public INamedTypeSymbol PXRowDeletedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowDeletedEventArgs)!;
		public INamedTypeSymbol PXRowPersistingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowPersistingEventArgs)!;
		public INamedTypeSymbol PXRowPersistedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXRowPersistedEventArgs)!;

		public INamedTypeSymbol PXFieldSelectingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXFieldSelectingEventArgs)!;
		public INamedTypeSymbol PXFieldDefaultingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXFieldDefaultingEventArgs)!;
		public INamedTypeSymbol PXFieldVerifyingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXFieldVerifyingEventArgs)!;
		public INamedTypeSymbol PXFieldUpdatingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXFieldUpdatingEventArgs)!;
		public INamedTypeSymbol PXFieldUpdatedEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXFieldUpdatedEventArgs)!;
		public INamedTypeSymbol PXCommandPreparingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXCommandPreparingEventArgs)!;
		public INamedTypeSymbol PXExceptionHandlingEventArgs => Compilation.GetTypeByMetadataName(EventArgsNames.PXExceptionHandlingEventArgs)!;

		public INamedTypeSymbol CacheAttached => Compilation.GetTypeByMetadataName(EventsNames.CacheAttached)!;
		public INamedTypeSymbol RowSelecting => Compilation.GetTypeByMetadataName(EventsNames.RowSelecting)!;
		public INamedTypeSymbol RowSelected => Compilation.GetTypeByMetadataName(EventsNames.RowSelected)!;
		public INamedTypeSymbol RowInserting => Compilation.GetTypeByMetadataName(EventsNames.RowInserting)!;
		public INamedTypeSymbol RowInserted => Compilation.GetTypeByMetadataName(EventsNames.RowInserted)!;
		public INamedTypeSymbol RowUpdating => Compilation.GetTypeByMetadataName(EventsNames.RowUpdating)!;
		public INamedTypeSymbol RowUpdated => Compilation.GetTypeByMetadataName(EventsNames.RowUpdated)!;
		public INamedTypeSymbol RowDeleting => Compilation.GetTypeByMetadataName(EventsNames.RowDeleting)!;
		public INamedTypeSymbol RowDeleted => Compilation.GetTypeByMetadataName(EventsNames.RowDeleted)!;
		public INamedTypeSymbol RowPersisting => Compilation.GetTypeByMetadataName(EventsNames.RowPersisting)!;
		public INamedTypeSymbol RowPersisted => Compilation.GetTypeByMetadataName(EventsNames.RowPersisted)!;

		public INamedTypeSymbol FieldSelecting => Compilation.GetTypeByMetadataName(EventsNames.FieldSelecting)!;
		public INamedTypeSymbol FieldDefaulting => Compilation.GetTypeByMetadataName(EventsNames.FieldDefaulting)!;
		public INamedTypeSymbol FieldVerifying => Compilation.GetTypeByMetadataName(EventsNames.FieldVerifying)!;
		public INamedTypeSymbol FieldUpdating => Compilation.GetTypeByMetadataName(EventsNames.FieldUpdating)!;
		public INamedTypeSymbol FieldUpdated => Compilation.GetTypeByMetadataName(EventsNames.FieldUpdated)!;
		public INamedTypeSymbol CommandPreparing => Compilation.GetTypeByMetadataName(EventsNames.CommandPreparing)!;
		public INamedTypeSymbol ExceptionHandling => Compilation.GetTypeByMetadataName(EventsNames.ExceptionHandling1)!;

		public INamedTypeSymbol FieldSelectingTypedRow => Compilation.GetTypeByMetadataName(EventsNames.FieldSelecting2)!;
		public INamedTypeSymbol FieldDefaultingTypedRow => Compilation.GetTypeByMetadataName(EventsNames.FieldDefaulting2)!;
		public INamedTypeSymbol FieldVerifyingTypedRow => Compilation.GetTypeByMetadataName(EventsNames.FieldVerifying2)!;
		public INamedTypeSymbol FieldUpdatingTypedRow => Compilation.GetTypeByMetadataName(EventsNames.FieldUpdating2)!;
		public INamedTypeSymbol FieldUpdatedTypedRow => Compilation.GetTypeByMetadataName(EventsNames.FieldUpdated2)!;
		public INamedTypeSymbol ExceptionHandlingTypedRow => Compilation.GetTypeByMetadataName(EventsNames.ExceptionHandling2)!;

		private static IReadOnlyDictionary<ITypeSymbol, EventType> CreateEventTypeMap(EventSymbols eventSymbols)
		{
			var map = new Dictionary<ITypeSymbol, EventType>(SymbolEqualityComparer.Default)
				{
					{ eventSymbols.PXRowSelectingEventArgs, EventType.RowSelecting },
					{ eventSymbols.PXRowSelectedEventArgs, EventType.RowSelected },
					{ eventSymbols.PXRowInsertingEventArgs, EventType.RowInserting },
					{ eventSymbols.PXRowInsertedEventArgs, EventType.RowInserted },
					{ eventSymbols.PXRowUpdatingEventArgs, EventType.RowUpdating },
					{ eventSymbols.PXRowUpdatedEventArgs, EventType.RowUpdated },
					{ eventSymbols.PXRowDeletingEventArgs, EventType.RowDeleting },
					{ eventSymbols.PXRowDeletedEventArgs, EventType.RowDeleted },
					{ eventSymbols.PXRowPersistingEventArgs, EventType.RowPersisting },
					{ eventSymbols.PXRowPersistedEventArgs, EventType.RowPersisted },
					{ eventSymbols.PXFieldSelectingEventArgs, EventType.FieldSelecting },
					{ eventSymbols.PXFieldDefaultingEventArgs, EventType.FieldDefaulting },
					{ eventSymbols.PXFieldVerifyingEventArgs, EventType.FieldVerifying },
					{ eventSymbols.PXFieldUpdatingEventArgs, EventType.FieldUpdating },
					{ eventSymbols.PXFieldUpdatedEventArgs, EventType.FieldUpdated },
					{ eventSymbols.PXCommandPreparingEventArgs, EventType.CommandPreparing },
					{ eventSymbols.PXExceptionHandlingEventArgs, EventType.ExceptionHandling },

					{ eventSymbols.CacheAttached, EventType.CacheAttached },
					{ eventSymbols.RowSelecting, EventType.RowSelecting },
					{ eventSymbols.RowSelected, EventType.RowSelected },
					{ eventSymbols.RowInserting, EventType.RowInserting },
					{ eventSymbols.RowInserted, EventType.RowInserted },
					{ eventSymbols.RowUpdating, EventType.RowUpdating },
					{ eventSymbols.RowUpdated, EventType.RowUpdated },
					{ eventSymbols.RowDeleting, EventType.RowDeleting },
					{ eventSymbols.RowDeleted, EventType.RowDeleted },
					{ eventSymbols.RowPersisting, EventType.RowPersisting },
					{ eventSymbols.RowPersisted, EventType.RowPersisted },
					{ eventSymbols.FieldSelecting, EventType.FieldSelecting },
					{ eventSymbols.FieldDefaulting, EventType.FieldDefaulting },
					{ eventSymbols.FieldVerifying, EventType.FieldVerifying },
					{ eventSymbols.FieldUpdating, EventType.FieldUpdating },
					{ eventSymbols.FieldUpdated, EventType.FieldUpdated },
					{ eventSymbols.CommandPreparing, EventType.CommandPreparing },
					{ eventSymbols.ExceptionHandling, EventType.ExceptionHandling },
				};

			// These symbols can be absent on some versions of Acumatica
			map.TryAdd(eventSymbols.FieldSelectingTypedRow, EventType.FieldSelecting);
			map.TryAdd(eventSymbols.FieldDefaultingTypedRow, EventType.FieldDefaulting);
			map.TryAdd(eventSymbols.FieldVerifyingTypedRow, EventType.FieldVerifying);
			map.TryAdd(eventSymbols.FieldUpdatingTypedRow, EventType.FieldUpdating);
			map.TryAdd(eventSymbols.FieldUpdatedTypedRow, EventType.FieldUpdated);
			map.TryAdd(eventSymbols.ExceptionHandlingTypedRow, EventType.ExceptionHandling);

			return map;
		}

		private static IReadOnlyDictionary<EventHandlerGeneralInfo, INamedTypeSymbol> CreateEventHandlerSignatureTypeMap(EventSymbols eventSymbols)
		{
			return new Dictionary<EventHandlerGeneralInfo, INamedTypeSymbol>()
			{
				{ new EventHandlerGeneralInfo(EventType.RowSelecting, EventHandlerSignatureType.Classic), eventSymbols.PXRowSelectingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowSelected, EventHandlerSignatureType.Classic), eventSymbols.PXRowSelectedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowInserting, EventHandlerSignatureType.Classic), eventSymbols.PXRowInsertingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowInserted, EventHandlerSignatureType.Classic), eventSymbols.PXRowInsertedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowUpdating, EventHandlerSignatureType.Classic), eventSymbols.PXRowUpdatingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowUpdated, EventHandlerSignatureType.Classic), eventSymbols.PXRowUpdatedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowDeleting, EventHandlerSignatureType.Classic), eventSymbols.PXRowDeletingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowDeleted, EventHandlerSignatureType.Classic), eventSymbols.PXRowDeletedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowPersisting, EventHandlerSignatureType.Classic), eventSymbols.PXRowPersistingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.RowPersisted, EventHandlerSignatureType.Classic), eventSymbols.PXRowPersistedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.FieldSelecting, EventHandlerSignatureType.Classic), eventSymbols.PXFieldSelectingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.FieldDefaulting, EventHandlerSignatureType.Classic), eventSymbols.PXFieldDefaultingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.FieldVerifying, EventHandlerSignatureType.Classic), eventSymbols.PXFieldVerifyingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.FieldUpdating, EventHandlerSignatureType.Classic), eventSymbols.PXFieldUpdatingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.FieldUpdated, EventHandlerSignatureType.Classic), eventSymbols.PXFieldUpdatedEventArgs },
				{ new EventHandlerGeneralInfo(EventType.CommandPreparing, EventHandlerSignatureType.Classic), eventSymbols.PXCommandPreparingEventArgs },
				{ new EventHandlerGeneralInfo(EventType.ExceptionHandling, EventHandlerSignatureType.Classic), eventSymbols.PXExceptionHandlingEventArgs },

				{ new EventHandlerGeneralInfo(EventType.CacheAttached, EventHandlerSignatureType.Generic), eventSymbols.CacheAttached },
				{ new EventHandlerGeneralInfo(EventType.RowSelecting, EventHandlerSignatureType.Generic), eventSymbols.RowSelecting },
				{ new EventHandlerGeneralInfo(EventType.RowSelected, EventHandlerSignatureType.Generic), eventSymbols.RowSelected },
				{ new EventHandlerGeneralInfo(EventType.RowInserting, EventHandlerSignatureType.Generic), eventSymbols.RowInserting },
				{ new EventHandlerGeneralInfo(EventType.RowInserted, EventHandlerSignatureType.Generic), eventSymbols.RowInserted },
				{ new EventHandlerGeneralInfo(EventType.RowUpdating, EventHandlerSignatureType.Generic), eventSymbols.RowUpdating },
				{ new EventHandlerGeneralInfo(EventType.RowUpdated, EventHandlerSignatureType.Generic), eventSymbols.RowUpdated },
				{ new EventHandlerGeneralInfo(EventType.RowDeleting, EventHandlerSignatureType.Generic), eventSymbols.RowDeleting },
				{ new EventHandlerGeneralInfo(EventType.RowDeleted, EventHandlerSignatureType.Generic), eventSymbols.RowDeleted },
				{ new EventHandlerGeneralInfo(EventType.RowPersisting, EventHandlerSignatureType.Generic), eventSymbols.RowPersisting },
				{ new EventHandlerGeneralInfo(EventType.RowPersisted, EventHandlerSignatureType.Generic), eventSymbols.RowPersisted },
				{ new EventHandlerGeneralInfo(EventType.FieldSelecting, EventHandlerSignatureType.Generic), eventSymbols.FieldSelectingTypedRow ?? eventSymbols.FieldSelecting },
				{ new EventHandlerGeneralInfo(EventType.FieldDefaulting, EventHandlerSignatureType.Generic), eventSymbols.FieldDefaultingTypedRow ?? eventSymbols.FieldDefaulting },
				{ new EventHandlerGeneralInfo(EventType.FieldVerifying, EventHandlerSignatureType.Generic), eventSymbols.FieldVerifyingTypedRow ?? eventSymbols.FieldVerifying },
				{ new EventHandlerGeneralInfo(EventType.FieldUpdating, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatingTypedRow ?? eventSymbols.FieldUpdating },
				{ new EventHandlerGeneralInfo(EventType.FieldUpdated, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatedTypedRow ?? eventSymbols.FieldUpdated },
				{ new EventHandlerGeneralInfo(EventType.CommandPreparing, EventHandlerSignatureType.Generic), eventSymbols.CommandPreparing },
				{ new EventHandlerGeneralInfo(EventType.ExceptionHandling, EventHandlerSignatureType.Generic), eventSymbols.ExceptionHandlingTypedRow ?? eventSymbols.ExceptionHandling },
			};
		}
	}
}
