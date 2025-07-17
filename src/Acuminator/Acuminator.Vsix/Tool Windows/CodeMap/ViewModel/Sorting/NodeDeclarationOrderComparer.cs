#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap;

/// <summary>
/// A code map tree node declaration order comparer.
/// </summary>
internal class NodeDeclarationOrderComparer : IComparer<TreeNodeViewModel>
{
	public static readonly NodeDeclarationOrderComparer Instance = new();

	private  NodeDeclarationOrderComparer() { }

	public int Compare(TreeNodeViewModel x, TreeNodeViewModel y)
	{
		if (ReferenceEquals(x, y)) 
			return 0;

		var nodeWithOrderX = x as IHaveDeclarationOrder;
		var nodeWithOrderY = y as IHaveDeclarationOrder;

		if (nodeWithOrderX == null && nodeWithOrderY == null)		//Always place nodes without symbol first
			return 0;
		else if (nodeWithOrderX == null)
			return 1;
		else if (nodeWithOrderY == null)
			return -1;

		return nodeWithOrderX.DeclarationOrder - nodeWithOrderY.DeclarationOrder;
	}
}