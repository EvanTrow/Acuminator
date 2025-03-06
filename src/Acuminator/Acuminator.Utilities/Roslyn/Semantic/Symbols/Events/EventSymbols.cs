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
			_eventHandlerSignatureTypeMap = new Lazy<IReadOnlyDictionary<EventHandlerLooseInfo, INamedTypeSymbol>>(
				() => CreateEventHandlerSignatureTypeMap(this));
		}

		private readonly Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>> _eventTypeMap;
		public IReadOnlyDictionary<ITypeSymbol, EventType> EventTypeMap => _eventTypeMap.Value;

		private readonly Lazy<IReadOnlyDictionary<EventHandlerLooseInfo, INamedTypeSymbol>> _eventHandlerSignatureTypeMap;
		public IReadOnlyDictionary<EventHandlerLooseInfo, INamedTypeSymbol> EventHandlerSignatureTypeMap => _eventHandlerSignatureTypeMap.Value;

		public INamedTypeSymbol PXRowSelectingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowSelectingEventArgs)!;
		public INamedTypeSymbol PXRowSelectedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowSelectedEventArgs)!;
		public INamedTypeSymbol PXRowInsertingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowInsertingEventArgs)!;
		public INamedTypeSymbol PXRowInsertedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowInsertedEventArgs)!;
		public INamedTypeSymbol PXRowUpdatingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowUpdatingEventArgs)!;
		public INamedTypeSymbol PXRowUpdatedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowUpdatedEventArgs)!;
		public INamedTypeSymbol PXRowDeletingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowDeletingEventArgs)!;
		public INamedTypeSymbol PXRowDeletedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowDeletedEventArgs)!;
		public INamedTypeSymbol PXRowPersistingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowPersistingEventArgs)!;
		public INamedTypeSymbol PXRowPersistedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXRowPersistedEventArgs)!;

		public INamedTypeSymbol PXFieldSelectingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXFieldSelectingEventArgs)!;
		public INamedTypeSymbol PXFieldDefaultingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXFieldDefaultingEventArgs)!;
		public INamedTypeSymbol PXFieldVerifyingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXFieldVerifyingEventArgs)!;
		public INamedTypeSymbol PXFieldUpdatingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXFieldUpdatingEventArgs)!;
		public INamedTypeSymbol PXFieldUpdatedEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXFieldUpdatedEventArgs)!;
		public INamedTypeSymbol PXCommandPreparingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXCommandPreparingEventArgs)!;
		public INamedTypeSymbol PXExceptionHandlingEventArgs => Compilation.GetTypeByMetadataName(Events.ArgsNames.PXExceptionHandlingEventArgs)!;

		public INamedTypeSymbol CacheAttached => Compilation.GetTypeByMetadataName(Events.Names.CacheAttached)!;
		public INamedTypeSymbol RowSelecting => Compilation.GetTypeByMetadataName(Events.Names.RowSelecting)!;
		public INamedTypeSymbol RowSelected => Compilation.GetTypeByMetadataName(Events.Names.RowSelected)!;
		public INamedTypeSymbol RowInserting => Compilation.GetTypeByMetadataName(Events.Names.RowInserting)!;
		public INamedTypeSymbol RowInserted => Compilation.GetTypeByMetadataName(Events.Names.RowInserted)!;
		public INamedTypeSymbol RowUpdating => Compilation.GetTypeByMetadataName(Events.Names.RowUpdating)!;
		public INamedTypeSymbol RowUpdated => Compilation.GetTypeByMetadataName(Events.Names.RowUpdated)!;
		public INamedTypeSymbol RowDeleting => Compilation.GetTypeByMetadataName(Events.Names.RowDeleting)!;
		public INamedTypeSymbol RowDeleted => Compilation.GetTypeByMetadataName(Events.Names.RowDeleted)!;
		public INamedTypeSymbol RowPersisting => Compilation.GetTypeByMetadataName(Events.Names.RowPersisting)!;
		public INamedTypeSymbol RowPersisted => Compilation.GetTypeByMetadataName(Events.Names.RowPersisted)!;

		public INamedTypeSymbol FieldSelecting => Compilation.GetTypeByMetadataName(Events.Names.FieldSelecting)!;
		public INamedTypeSymbol FieldDefaulting => Compilation.GetTypeByMetadataName(Events.Names.FieldDefaulting)!;
		public INamedTypeSymbol FieldVerifying => Compilation.GetTypeByMetadataName(Events.Names.FieldVerifying)!;
		public INamedTypeSymbol FieldUpdating => Compilation.GetTypeByMetadataName(Events.Names.FieldUpdating)!;
		public INamedTypeSymbol FieldUpdated => Compilation.GetTypeByMetadataName(Events.Names.FieldUpdated)!;
		public INamedTypeSymbol CommandPreparing => Compilation.GetTypeByMetadataName(Events.Names.CommandPreparing)!;
		public INamedTypeSymbol ExceptionHandling => Compilation.GetTypeByMetadataName(Events.Names.ExceptionHandling1)!;

		public INamedTypeSymbol FieldSelectingTypedRow => Compilation.GetTypeByMetadataName(Events.Names.FieldSelecting2)!;
		public INamedTypeSymbol FieldDefaultingTypedRow => Compilation.GetTypeByMetadataName(Events.Names.FieldDefaulting2)!;
		public INamedTypeSymbol FieldVerifyingTypedRow => Compilation.GetTypeByMetadataName(Events.Names.FieldVerifying2)!;
		public INamedTypeSymbol FieldUpdatingTypedRow => Compilation.GetTypeByMetadataName(Events.Names.FieldUpdating2)!;
		public INamedTypeSymbol FieldUpdatedTypedRow => Compilation.GetTypeByMetadataName(Events.Names.FieldUpdated2)!;
		public INamedTypeSymbol ExceptionHandlingTypedRow => Compilation.GetTypeByMetadataName(Events.Names.ExceptionHandling2)!;

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

		private static IReadOnlyDictionary<EventHandlerLooseInfo, INamedTypeSymbol> CreateEventHandlerSignatureTypeMap(EventSymbols eventSymbols)
		{
			return new Dictionary<EventHandlerLooseInfo, INamedTypeSymbol>()
			{
				{ new EventHandlerLooseInfo(EventType.RowSelecting, EventHandlerSignatureType.Classic), eventSymbols.PXRowSelectingEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowSelected, EventHandlerSignatureType.Classic), eventSymbols.PXRowSelectedEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowInserting, EventHandlerSignatureType.Classic), eventSymbols.PXRowInsertingEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowInserted, EventHandlerSignatureType.Classic), eventSymbols.PXRowInsertedEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowUpdating, EventHandlerSignatureType.Classic), eventSymbols.PXRowUpdatingEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowUpdated, EventHandlerSignatureType.Classic), eventSymbols.PXRowUpdatedEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowDeleting, EventHandlerSignatureType.Classic), eventSymbols.PXRowDeletingEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowDeleted, EventHandlerSignatureType.Classic), eventSymbols.PXRowDeletedEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowPersisting, EventHandlerSignatureType.Classic), eventSymbols.PXRowPersistingEventArgs },
				{ new EventHandlerLooseInfo(EventType.RowPersisted, EventHandlerSignatureType.Classic), eventSymbols.PXRowPersistedEventArgs },
				{ new EventHandlerLooseInfo(EventType.FieldSelecting, EventHandlerSignatureType.Classic), eventSymbols.PXFieldSelectingEventArgs },
				{ new EventHandlerLooseInfo(EventType.FieldDefaulting, EventHandlerSignatureType.Classic), eventSymbols.PXFieldDefaultingEventArgs },
				{ new EventHandlerLooseInfo(EventType.FieldVerifying, EventHandlerSignatureType.Classic), eventSymbols.PXFieldVerifyingEventArgs },
				{ new EventHandlerLooseInfo(EventType.FieldUpdating, EventHandlerSignatureType.Classic), eventSymbols.PXFieldUpdatingEventArgs },
				{ new EventHandlerLooseInfo(EventType.FieldUpdated, EventHandlerSignatureType.Classic), eventSymbols.PXFieldUpdatedEventArgs },
				{ new EventHandlerLooseInfo(EventType.CommandPreparing, EventHandlerSignatureType.Classic), eventSymbols.PXCommandPreparingEventArgs },
				{ new EventHandlerLooseInfo(EventType.ExceptionHandling, EventHandlerSignatureType.Classic), eventSymbols.PXExceptionHandlingEventArgs },

				{ new EventHandlerLooseInfo(EventType.CacheAttached, EventHandlerSignatureType.Generic), eventSymbols.CacheAttached },
				{ new EventHandlerLooseInfo(EventType.RowSelecting, EventHandlerSignatureType.Generic), eventSymbols.RowSelecting },
				{ new EventHandlerLooseInfo(EventType.RowSelected, EventHandlerSignatureType.Generic), eventSymbols.RowSelected },
				{ new EventHandlerLooseInfo(EventType.RowInserting, EventHandlerSignatureType.Generic), eventSymbols.RowInserting },
				{ new EventHandlerLooseInfo(EventType.RowInserted, EventHandlerSignatureType.Generic), eventSymbols.RowInserted },
				{ new EventHandlerLooseInfo(EventType.RowUpdating, EventHandlerSignatureType.Generic), eventSymbols.RowUpdating },
				{ new EventHandlerLooseInfo(EventType.RowUpdated, EventHandlerSignatureType.Generic), eventSymbols.RowUpdated },
				{ new EventHandlerLooseInfo(EventType.RowDeleting, EventHandlerSignatureType.Generic), eventSymbols.RowDeleting },
				{ new EventHandlerLooseInfo(EventType.RowDeleted, EventHandlerSignatureType.Generic), eventSymbols.RowDeleted },
				{ new EventHandlerLooseInfo(EventType.RowPersisting, EventHandlerSignatureType.Generic), eventSymbols.RowPersisting },
				{ new EventHandlerLooseInfo(EventType.RowPersisted, EventHandlerSignatureType.Generic), eventSymbols.RowPersisted },
				{ new EventHandlerLooseInfo(EventType.FieldSelecting, EventHandlerSignatureType.Generic), eventSymbols.FieldSelectingTypedRow ?? eventSymbols.FieldSelecting },
				{ new EventHandlerLooseInfo(EventType.FieldDefaulting, EventHandlerSignatureType.Generic), eventSymbols.FieldDefaultingTypedRow ?? eventSymbols.FieldDefaulting },
				{ new EventHandlerLooseInfo(EventType.FieldVerifying, EventHandlerSignatureType.Generic), eventSymbols.FieldVerifyingTypedRow ?? eventSymbols.FieldVerifying },
				{ new EventHandlerLooseInfo(EventType.FieldUpdating, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatingTypedRow ?? eventSymbols.FieldUpdating },
				{ new EventHandlerLooseInfo(EventType.FieldUpdated, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatedTypedRow ?? eventSymbols.FieldUpdated },
				{ new EventHandlerLooseInfo(EventType.CommandPreparing, EventHandlerSignatureType.Generic), eventSymbols.CommandPreparing },
				{ new EventHandlerLooseInfo(EventType.ExceptionHandling, EventHandlerSignatureType.Generic), eventSymbols.ExceptionHandlingTypedRow ?? eventSymbols.ExceptionHandling },
			};
		}
	}
}
