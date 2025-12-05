using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic;

public sealed class OverridableItemsCollection<TInfo> : Dictionary<string, TInfo>
where TInfo : IOverridableItem<TInfo>
{
	public IEnumerable<TInfo> Items => Values;

	public OverridableItemsCollection() : base(StringComparer.OrdinalIgnoreCase)
	{
	}

	public OverridableItemsCollection(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
	{
	}

	internal void Add<TWriteableInfo>(TWriteableInfo info)
	where TWriteableInfo : TInfo, IWriteableBaseItem<TInfo>
	{
		if (info?.Name == null)
		{
			throw new ArgumentNullException($"{nameof(info)}.{nameof(IOverridableItem<TInfo>.Name)}");
		}

		if (TryGetValue(info.Name, out TInfo existingValue))
		{
			if (!ReferenceEquals(existingValue, info))
			{
				info.Base = existingValue;
				base[info.Name] = info;
			}
		}
		else
		{
			Add(info.Name, info);
		}
	}
}
