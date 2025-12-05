namespace Acuminator.Utilities.Roslyn.Semantic;

/// <summary>
/// An interface for an object that has a declaration order.
/// </summary>
public interface IHaveDeclarationOrder
{
	/// <summary>
	/// Gets the declaration order.
	/// </summary>
	/// <value>
	/// The declaration order.
	/// </value>
	int DeclarationOrder { get; }
}
