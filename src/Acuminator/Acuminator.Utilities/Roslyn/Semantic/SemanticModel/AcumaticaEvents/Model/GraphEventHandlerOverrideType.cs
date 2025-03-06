namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Type of the graph event handler override.
/// </summary>
public enum GraphEventHandlerOverrideType : byte
{
	/// <summary>
	/// Handler is not an override.
	/// </summary>
	None,

	/// <summary>
	/// Handler is a C# override.
	/// </summary>
	CSharp,

	/// <summary>
	/// Handler is an override implemented with Acumatica events override mechanism.
	/// </summary>
	AcumaticaEventsOverride
}
