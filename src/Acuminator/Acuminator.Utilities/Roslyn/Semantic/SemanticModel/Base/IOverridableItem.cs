namespace Acuminator.Utilities.Roslyn.Semantic;

/// <summary>
/// An interface for a DTO which stores info about some item. The item which can be overridable, and the info about base item is also stored.
/// </summary>
/// <typeparam name="T">Generic type parameter.</typeparam>
public interface IOverridableItem<out T>
where T : IOverridableItem<T>
{
	string Name { get; }

	/// <summary>
	/// The overridden base info if any.
	/// </summary>
	T? Base { get; }

	/// <summary>
	/// Combine this info with info from base type.
	/// </summary>
	void CombineWithBaseInfo();
}

internal interface IWriteableBaseItem<T> : IOverridableItem<T>
where T : IWriteableBaseItem<T>
{
	/// <inheritdoc cref="IOverridableItem{T}.Base"/>
	new T? Base
	{
		set;
	}
}
