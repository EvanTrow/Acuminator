namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

/// <summary>
/// Values that represent different kinds of results for the collection of type hierarchy and infer of the semantic data from it.
/// </summary>
/// <remarks>
/// The term <b>"type hierarchy"</b> should be understood with the Acumatica Framework context in mind.<br/>
/// Type hierarchy here means:
/// <list type="bullet">
/// <item>
/// For a graph type it is the type itself plus all its base concrete graph types up to the trivial <c>PXGraph&lt;TGraph&gt;</c> type (the trivial type is not included).
/// </item>
/// <item>
/// For a graph extension it is the type itself, all its base concrete graph extension types up to the trivial <c>PXGraphExtension</c> types (the trivial types are not included),<br/>
/// type hierarchies of all graphs that are extended by the graph extension or by any of its ancestor graph extensions in the type hierarchy,<br/>
/// and type hierarchies of all lower-level graph extensions that are extended by the graph extension or by any of its ancestor graph extensions in the type hierarchy,
/// </item>
/// </list>
/// </remarks>
public enum InferResultKind : byte
{
	/// <summary>
	/// Acuminator failed to collect analyze type hierarchy for this object and infer semantic info.<br/>
	/// This option indicates any type of failure that is different from <see cref="CircularReferences"/> and <see cref="MultipleRootTypes"/>.
	/// </summary>
	UnrecognizedError,

	/// <summary>
	/// The type used to infer information has circular references in the type hierarchy.
	/// </summary>
	CircularReferences,

	/// <summary>
	/// The type used to infer information has extension type bad base extensions in the type hierarchy.
	/// </summary>
	BadBaseExtensions,

	/// <summary>
	/// The collection of type hierarchy and infer of the semantic info from it was completed successfully.
	/// </summary>
	Success
}