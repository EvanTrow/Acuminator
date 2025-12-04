using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic;

/// <summary>
/// Generic class for overridable semantic info from a graph or DAC with symbol.
/// </summary>
/// <typeparam name="TInfo">Type of the information symbol.</typeparam>
/// <typeparam name="S">Type of the declaration symbol of the item.</typeparam>
public abstract class OverridableSymbolItem<TInfo, S> : SymbolItem<S>, IWriteableBaseItem<TInfo>
where TInfo : OverridableSymbolItem<TInfo, S>
where S : ISymbol
{
	protected TInfo? _baseInfo;

	public TInfo? Base => _baseInfo;

	TInfo? IWriteableBaseItem<TInfo>.Base
	{
		set 
		{
			_baseInfo = value;

			if (value != null)
				CombineWithBaseInfo();
		}
	}

	protected OverridableSymbolItem(S symbol, int declarationOrder, TInfo baseInfo) : this(symbol, declarationOrder)
	{
		_baseInfo = baseInfo.CheckIfNull();
	}

	protected OverridableSymbolItem(S symbol, int declarationOrder) : base(symbol, declarationOrder)
	{
	}

	void IOverridableItem<TInfo>.CombineWithBaseInfo() => 
		CombineWithBaseInfo();

	/// <inheritdoc cref="IOverridableItem{T}.CombineWithBaseInfo()"/>
	protected abstract void CombineWithBaseInfo();
}
