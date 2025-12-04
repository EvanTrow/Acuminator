using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public static class GraphViewSymbolUtils
{
	/// <summary>
	/// Returns true if the data view is a processing view
	/// </summary>
	/// <param name="view">The type symbol of a data view</param>
	/// <param name="pxContext">The context</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsProcessingView(this ITypeSymbol view, PXContext pxContext)
	{
		pxContext.ThrowOnNull();

		return view.CheckIfNull().InheritsFromOrEqualsGeneric(pxContext.PXProcessingBase.Type);
	}
}