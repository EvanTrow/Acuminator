using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SecondLevelGraphExtension : PXGraphExtension<SecondIndependentGraphExtension, FirstIndependentGraphExtension, MyGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SecondIndependentGraphExtension : PXGraphExtension<MyGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class FirstIndependentGraphExtension : PXGraphExtension<MyGraph>
	{
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}
