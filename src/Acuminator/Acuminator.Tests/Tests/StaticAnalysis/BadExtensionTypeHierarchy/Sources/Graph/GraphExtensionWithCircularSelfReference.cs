using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ExtensionA : PXGraphExtension<ExtensionA, MyGraph> { }

	public class MyGraph : PXGraph<MyGraph>
	{ }
}
