namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

/// <summary>
/// Types of the PXOverride mechanism that can be used by methods of graph extensions.
/// </summary>
public enum PXOverrideType : byte
{
	/// <summary>
	/// Method is not a PXOverride patch method.
	/// </summary>
	None,

	/// <summary>
	/// Method is a PXOverride patch method without a base delegate as its last parameter.
	/// </summary>
	WithoutBaseDelegate,

	/// <summary>
	/// Method is a PXOverride patch method with a base delegate as its last parameter.<br/>
	/// Base delegate has a valid signature - its return type and parameters match method's return type and parameters.
	/// </summary>
	WithValidBaseDelegate,

	/// <summary>
	/// Method is a PXOverride patch method with a base delegate as its last parameter.<br/>
	/// Base delegate has an invalid signature - its return type and parameters do not match method's return type and parameters.
	/// </summary>
	WithInvalidBaseDelegate
}
