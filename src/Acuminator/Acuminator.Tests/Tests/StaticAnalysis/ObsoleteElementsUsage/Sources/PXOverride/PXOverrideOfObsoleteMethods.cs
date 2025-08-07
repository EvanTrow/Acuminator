using System;
using System.Collections.Generic;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public object TestMethod1(int x, bool drilldown, double y, Func<int, bool, double, object> del)
		{
			return del(x, drilldown, y);
		}

		[PXOverride]
		public void TestMethod2(int x)
		{
		}

		[PXOverride]
		public void TestMethod3(IEnumerable<string> source, Action<IEnumerable<string>> del)
		{
			del(source);
		}

		[PXOverride]
		public void TestMethod4(Action del)
		{
			del();
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		[PXOverride]
		public object TestMethod1(int x, bool drilldown, double y)
		{
			return new object();
		}
	}


	public class MyGraph : PXGraph<MyGraph>
	{
		[Obsolete]
		public virtual object TestMethod1(int x, bool drilldown, double y)
		{
			return new object();
		}

		[Obsolete("Obsolete")]
		public virtual void TestMethod2(int x)
		{	
		}

		[Obsolete("Obsolete", error: true)]
		public virtual void TestMethod3(IEnumerable<string> source)
		{
		}

		[Obsolete]
		public virtual void TestMethod4()
		{
		}
	}
}