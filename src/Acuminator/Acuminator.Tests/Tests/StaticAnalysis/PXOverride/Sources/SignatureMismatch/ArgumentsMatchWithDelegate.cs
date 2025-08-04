using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual object TestMethod(int x, bool drilldown)
		{
			return new object();
		}

		[PXOverride]
		public void TestMethodFromGraph(object a, Action<object> baseAction) { }
	}

	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public object TestMethod(int x, bool drilldown, Func<int, bool, object> del)
		{
			return new object();
		}

		[PXOverride]
		public void TestMethodFromGraph(object a, Action<object> baseAction) { }
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		public virtual void TestMethodFromGraph(object a) { }
	}
}