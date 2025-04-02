namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Represents a kind of the event target in the Acumatica Event Model.
/// </summary>
public enum EventTargetKind : byte
{
	/// <summary>
	/// Event has no target. Usually, this means that there is something wrong with the event.
	/// </summary>
	None,

	/// <summary>
	/// Event targets entire DAC row. Examples: row selected event, row inserted event.
	/// </summary>
	Row,

	/// <summary>
	/// Event targets a DAC field. Examples: field updated event, field defaulting event.
	/// </summary>
	Field
}
