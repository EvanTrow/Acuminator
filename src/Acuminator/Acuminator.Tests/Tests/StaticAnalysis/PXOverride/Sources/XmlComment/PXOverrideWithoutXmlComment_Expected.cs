using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{

		/// Overrides <seealso cref="BaseExtension.TestMethod1(int, bool, double)"/>
		[PXOverride]
		public object TestMethod1(int x, bool drilldown, double y, Func<int, bool, double, object> base_TestMethod1)
		{
			return base_TestMethod1(x, drilldown, y);
		}

		/// Overrides <seealso cref="MyGraph.TestMethod2(int)"/>
		[PXOverride]
		public void TestMethod2(int x, Action<int> base_TestMethod2)
		{
			base_TestMethod2(x);
		}

		/// Overrides <seealso cref="MyGraph.TestMethod3(List{Func{int}})"/>
		[PXOverride]
		public IEnumerable<int> TestMethod3(List<Func<int>> providers, 
											Func<List<Func<int>>, IEnumerable<int>> base_TestMethod3)
		{
			return base_TestMethod3(providers);
		}

		/// Overrides <seealso cref="MyGraph.TestMethod4"/>
		[PXOverride]
		public void TestMethod4(Action base_TestMethod4)
		{
			base_TestMethod4();
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual object TestMethod1(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		protected internal virtual void TestMethod2(int x)
		{
		}

		protected virtual IEnumerable<int> TestMethod3(List<Func<int>> providers) =>
			providers.Select(f => f()).ToList();

		public virtual void TestMethod4()
		{
		}
	}
}