using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	/// <exclude/>
	public class Extension1 : PXCacheExtension<Extension2, MyDac> { }

	/// <exclude/>
	public class Extension2 : PXCacheExtension<Extension1, MyDac> { }

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{ }
}
