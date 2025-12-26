using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	public class Extension1 : PXGraphExtension<Extension2, MyGraph> { }

	public class Extension2 : PXGraphExtension<Extension1, MyGraph> { }

	public class MyGraph : PXGraph<MyGraph>
	{ }
}
