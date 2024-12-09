#nullable enable

using System;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	public static class AcumaticaEventUtils
	{
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
	}
}