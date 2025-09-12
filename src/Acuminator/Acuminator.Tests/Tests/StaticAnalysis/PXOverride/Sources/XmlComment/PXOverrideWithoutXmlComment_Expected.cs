using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		/// <summary>
		/// Tests method 1 override.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="drilldown">True to drilldown.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="base_TestMethod1">The first base test method.</param>
		/// <returns>
		/// An object.
		/// </returns>
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

		/// <inheritdoc cref="MyGraph.TestMethod4"/>
		/// Overrides <seealso cref="MyGraph.TestMethod4"/>
		[PXOverride]
		public void TestMethod4(Action base_TestMethod4)
		{
			base_TestMethod4();
		}

		#region Test Method 5
		/// Overrides <seealso cref="MyGraph.TestMethod5(object)"/>
		[PXOverride]
		public int TestMethod5(object obj, Func<object, int> base_TestMethod5)
		{
			return base_TestMethod5(obj);
		}
		#endregion

		#region Test Method 6
		/// <summary>
		/// Tests method 6.
		/// </summary>
		/// <param name="s">The string.</param>
		/// Overrides <seealso cref="MyGraph.TestMethod6(string)"/>
		[PXOverride]
		public void TestMethod6(string s, Action<string> base_TestMethod6)
		{
			base_TestMethod6(s);
		}
		#endregion
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

		/// <summary>
		/// Tests method 4.
		/// </summary>
		public virtual void TestMethod4()
		{
		}

		public virtual int TestMethod5(object obj)
		{
			return 1;
		}

		public virtual void TestMethod6(string s)
		{
		}
	}
}