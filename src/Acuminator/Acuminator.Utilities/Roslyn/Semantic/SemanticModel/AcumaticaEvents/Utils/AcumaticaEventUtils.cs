using System;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	public static class AcumaticaEventUtils
	{
		/// <summary>
		/// Get event target kind by event type.
		/// </summary>
		/// <param name="eventType">The event type to check.</param>
		/// <returns>
		/// The event target kind from the given event type.
		/// </returns>
		public static EventTargetKind GetEventTargetKindByEventType(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
				case EventType.CacheAttached:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return EventTargetKind.Field;

				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return EventTargetKind.Row;

				case EventType.None:
				default:
					return EventTargetKind.None;
			}
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC field event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacFieldEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
				case EventType.CacheAttached:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC row event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacRowEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return true;
				default:
					return false;
			}
		}

		public static EventCollectionMode GetEventCollectionMode(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.RowDeleting:
				case EventType.RowInserting:
				case EventType.RowUpdating:
				case EventType.RowPersisting:
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return EventCollectionMode.AddedToBeginning;

				case EventType.RowSelecting:    // row selecting is an exception among -ing ending events
				case EventType.RowSelected:
				case EventType.RowInserted:
				case EventType.RowUpdated:
				case EventType.RowDeleted:
				case EventType.RowPersisted:
				case EventType.CacheAttached:
				case EventType.FieldUpdated:
					return EventCollectionMode.AddedToEnd;

				case EventType.None:
				default:
					return EventCollectionMode.None;
			}
		}
	}
}