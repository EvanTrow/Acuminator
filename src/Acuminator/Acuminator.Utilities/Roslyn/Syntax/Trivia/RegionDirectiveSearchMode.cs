using System;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.Roslyn.Syntax.Trivia;

/// <summary>
/// A bit-field of flags for searching specified region compiler directive kinds.
/// </summary>
[Flags]
public enum RegionDirectiveSearchMode : byte
{
	None		= 0,
	StartRegion = 0b001,
	EndRegion 	= 0b010,
	AllRegions 	= StartRegion | EndRegion
}


public static class RegionDirectiveSearchModeExtensions
{
	/// <summary>
	/// Determines whether the specified <paramref name="searchMode"/> includes search for the <c>#region</c> compiler directive.
	/// </summary>
	/// <param name="searchMode">The region directives search mode.</param>
	/// <returns>
	/// <c>true</c> if the search mode includes searching for the <c>#region</c> compiler directives. Otherwise, <c>false</c>.
	/// </returns>
	public static bool IsStartRegion(this RegionDirectiveSearchMode searchMode) =>
		searchMode.IncludesRegionKind(RegionDirectiveSearchMode.StartRegion);
	
	/// <summary>
	/// Determines whether the specified <paramref name="searchMode"/> includes search for the <c>#endregion</c> compiler directive.
	/// </summary>
	/// <param name="searchMode">The region directives search mode.</param>
	/// <returns>
	/// <c>true</c> if the search mode includes searching for the <c>#endregion</c> compiler directives. Otherwise, <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IncludesEndRegion(this RegionDirectiveSearchMode searchMode) =>
		searchMode.IncludesRegionKind(RegionDirectiveSearchMode.EndRegion);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IncludesRegionKind(this RegionDirectiveSearchMode searchMode, RegionDirectiveSearchMode regionKindToCheck) =>
		(searchMode & regionKindToCheck) == regionKindToCheck;
}