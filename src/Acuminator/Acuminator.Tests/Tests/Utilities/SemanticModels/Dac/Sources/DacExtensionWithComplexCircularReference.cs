using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class ExtensionA : PXCacheExtension<ExtensionB, MyDac> { }

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class ExtensionB : PXCacheExtension<ExtensionC, MyDac> { }

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class ExtensionC : PXCacheExtension<ExtensionA, MyDac> { }

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{ }
}
