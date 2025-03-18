namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Represents the event collection mode in the Acumatica Event Model.
/// </summary>
public enum EventCollectionMode : byte
{
	None,

	/// <summary>
	/// Event handlers during the collection are added to the end of the collection of event handlers. The collection is effectively a queue.<br/>
	/// This is the collection mode for any event whose name ends with -ed and for the RowSelecting event
	/// </summary>
	AddedToEnd,

	/// <summary>
	/// Event handlers during the collection are added to the beginning of the collection of event handlers. The collection is effectively a stack.<br/>
	/// This is the collection mode for any event whose name ends with -ing except the RowSelecting event
	/// </summary>
	AddedToBeginning
}
