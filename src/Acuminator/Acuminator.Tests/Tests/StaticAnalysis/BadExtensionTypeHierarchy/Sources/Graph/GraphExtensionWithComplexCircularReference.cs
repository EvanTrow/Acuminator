using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ExtensionA : PXGraphExtension<DerivedExtension, ExtensionB, MyGraph> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ExtensionB : PXGraphExtension<ExtensionC, MyGraph> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ExtensionC : PXGraphExtension<ExtensionA, MyGraph> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : BaseExtension { }

	public abstract class BaseExtension : PXGraphExtension<MyGraph> { }

	public class MyGraph : PXGraph<MyGraph>
	{ }
}
