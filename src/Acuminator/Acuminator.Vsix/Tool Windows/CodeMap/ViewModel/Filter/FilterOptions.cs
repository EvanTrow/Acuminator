#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Filter;

public class FilterOptions : IEquatable<FilterOptions>
{
	public static readonly FilterOptions NoFilter = new(null);

	[MemberNotNullWhen(returnValue: true, nameof(FilterPattern))]
	public bool HasFilter => FilterPattern != null;

	public string? FilterPattern { get; }
		
	public FilterOptions(string? filterPattern)
	{
		FilterPattern = filterPattern.IsNullOrWhiteSpace()
			? null
			: filterPattern.Trim();
	}

	public override string ToString() => $"Filter: {FilterPattern ?? string.Empty}";

	public override bool Equals(object obj) => Equals(obj as FilterOptions);

	public bool Equals(FilterOptions? other) => 
		other != null && string.Equals(FilterPattern, other.FilterPattern, StringComparison.Ordinal);

	public override int GetHashCode() => FilterPattern?.GetHashCode() ?? 0;
}
