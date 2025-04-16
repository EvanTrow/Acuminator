namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

public enum EventHandlerSignatureType : byte
{
	/// <summary>
	/// Method is not an event handler
	/// </summary>
	None,

	/// <summary>
	/// Classic signature based on the naming convention
	/// (e.g., <code>void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)</code>)
	/// </summary>
	Classic,

	/// <summary>
	/// Generic signature (e.g., <code>void _(Events.RowSelected&lt;ARInvoice e&gt;)</code>)
	/// </summary>
	Generic,
}
