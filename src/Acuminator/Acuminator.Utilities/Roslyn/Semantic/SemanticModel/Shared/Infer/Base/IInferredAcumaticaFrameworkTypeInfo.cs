using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

/// <summary>
/// Interface for inferred semantic information about Acumatica Framework type.
/// </summary>
public interface IInferredAcumaticaFrameworkTypeInfo
{
	/// <summary>
	/// <see langword="true"/> if this object has circular references in the type hierarchy; otherwise, <see langword="false"/>.
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
	bool HasCircularReferences { get; }

	/// <summary>
	/// <see langword="true"/> if this object has multiple root Acumatica Framework types in its type hierarchy; otherwise, <see langword="false"/>.
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
	/// <br/>
	/// By <b>"root"</b> Acumatica Framework type we understand the most basic non-trivial type in the type hierarchy that is still an Acumatica Framework type.<br/>
	/// <br/>
	/// For example, for graphs and graph extensions the trivial symbols are <c>PXGraph</c> and <c>PXGraph&lt;TGraph&gt;</c> because every graph is derived from them.<br/>
	/// The first graph in the type hierarchy that is not derived from these trivial symbols is considered a root graph type.<br/>
	/// There is no other type in the type hierarchy it derives from ot customizes.
	/// </remarks>
	bool HasMultipleRootTypes { get; }

	/// <summary>
	/// <see langword="true"/> if Acuminator failed to collect type hierarchy for this object; otherwise, <see langword="false"/>.<br/>
	/// This flag indicates any type of failure that is different from circular references and multiple root types.
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
	bool FailedToCollectTypeHierarchy { get; }
}