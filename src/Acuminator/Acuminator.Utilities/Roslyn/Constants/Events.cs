namespace Acuminator.Utilities.Roslyn.Constants
{
	public static class Events
	{
		public static class Names
		{
			/// <summary>
			/// Commonly accepted name of the graph event handler with generic signature.
			/// </summary>
			public const string CommonEventHandlerWithGenericSignatureName = "_";

			public const string CacheAttached 	   = "PX.Data.Events+CacheAttached`1";
			public const string RowSelecting 	   = "PX.Data.Events+RowSelecting`1";
			public const string RowSelected 	   = "PX.Data.Events+RowSelected`1";
			public const string RowInserting 	   = "PX.Data.Events+RowInserting`1";
			public const string RowInserted 	   = "PX.Data.Events+RowInserted`1";
			public const string RowUpdating 	   = "PX.Data.Events+RowUpdating`1";
			public const string RowUpdated 		   = "PX.Data.Events+RowUpdated`1";
			public const string RowDeleting 	   = "PX.Data.Events+RowDeleting`1";
			public const string RowDeleted 		   = "PX.Data.Events+RowDeleted`1";
			public const string RowPersisting 	   = "PX.Data.Events+RowPersisting`1";
			public const string RowPersisted 	   = "PX.Data.Events+RowPersisted`1";
			public const string FieldSelecting 	   = "PX.Data.Events+FieldSelecting`1";
			public const string FieldDefaulting    = "PX.Data.Events+FieldDefaulting`1";
			public const string FieldVerifying 	   = "PX.Data.Events+FieldVerifying`1";
			public const string FieldUpdating 	   = "PX.Data.Events+FieldUpdating`1";
			public const string FieldUpdated 	   = "PX.Data.Events+FieldUpdated`1";
			public const string CommandPreparing   = "PX.Data.Events+CommandPreparing`1";
			public const string ExceptionHandling1 = "PX.Data.Events+ExceptionHandling`1";
			public const string FieldSelecting2    = "PX.Data.Events+FieldSelecting`2";
			public const string FieldDefaulting2   = "PX.Data.Events+FieldDefaulting`2";
			public const string FieldVerifying2    = "PX.Data.Events+FieldVerifying`2";
			public const string FieldUpdating2 	   = "PX.Data.Events+FieldUpdating`2";
			public const string FieldUpdated2 	   = "PX.Data.Events+FieldUpdated`2";
			public const string CommandPreparing2  = "PX.Data.Events+CommandPreparing`2";
			public const string ExceptionHandling2 = "PX.Data.Events+ExceptionHandling`2";

			public static class PXCache
			{
				public const string RowSelectingWhileReading = "RowSelectingWhileReading";
			}
		}

		public static class ArgsNames
		{
			public static readonly string PXRowSelectingEventArgs 	   = "PX.Data.PXRowSelectingEventArgs";
			public static readonly string PXRowSelectedEventArgs 	   = "PX.Data.PXRowSelectedEventArgs";
			public static readonly string PXRowInsertingEventArgs 	   = "PX.Data.PXRowInsertingEventArgs";
			public static readonly string PXRowInsertedEventArgs 	   = "PX.Data.PXRowInsertedEventArgs";
			public static readonly string PXRowUpdatingEventArgs 	   = "PX.Data.PXRowUpdatingEventArgs";
			public static readonly string PXRowUpdatedEventArgs 	   = "PX.Data.PXRowUpdatedEventArgs";
			public static readonly string PXRowDeletingEventArgs 	   = "PX.Data.PXRowDeletingEventArgs";
			public static readonly string PXRowDeletedEventArgs 	   = "PX.Data.PXRowDeletedEventArgs";
			public static readonly string PXRowPersistingEventArgs 	   = "PX.Data.PXRowPersistingEventArgs";
			public static readonly string PXRowPersistedEventArgs 	   = "PX.Data.PXRowPersistedEventArgs";
			public static readonly string PXFieldSelectingEventArgs    = "PX.Data.PXFieldSelectingEventArgs";
			public static readonly string PXFieldDefaultingEventArgs   = "PX.Data.PXFieldDefaultingEventArgs";
			public static readonly string PXFieldVerifyingEventArgs    = "PX.Data.PXFieldVerifyingEventArgs";
			public static readonly string PXFieldUpdatingEventArgs 	   = "PX.Data.PXFieldUpdatingEventArgs";
			public static readonly string PXFieldUpdatedEventArgs 	   = "PX.Data.PXFieldUpdatedEventArgs";
			public static readonly string PXCommandPreparingEventArgs  = "PX.Data.PXCommandPreparingEventArgs";
			public static readonly string PXExceptionHandlingEventArgs = "PX.Data.PXExceptionHandlingEventArgs";
		}
	}
}
