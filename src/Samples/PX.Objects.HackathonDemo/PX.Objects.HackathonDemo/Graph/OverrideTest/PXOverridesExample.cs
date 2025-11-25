using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.HackathonDemo.OverrideTest
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedExtension : PXGraphExtension<DerivedExtension, MyGraph>
	{
		public delegate string CustomDelegateType(string input);

		[PXOverride]
		public MyDac TestMethod1(MyDac x, bool drilldown, double y, Func<MyDac, bool, double, MyDac> base_TestMethod1) => base_TestMethod1(x, drilldown, y);

		[PXOverride]
		public void TestMethod2(Func<string> func, IEnumerable<List<string>> itemsLists, Action<Func<string>, IEnumerable<List<string>>> base_TestMethod2)
		{
			base_TestMethod2(func, itemsLists);
		}

		[PXOverride]
		public object TestMethod3(Func<string> func, int x, IEnumerable<List<string>> itemsLists, Func<Func<string>, int, IEnumerable<List<string>>, object> base_TestMethod3)
		{
			return base_TestMethod3?.Invoke(func, x, itemsLists);
		}

		[PXOverride]
		public string TestMethod4(string x, CustomDelegateType base_TestMethod4)
		{
			if (base_TestMethod4 != null)
				return base_TestMethod4(x);
			else
				return string.Empty;
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : BaseExtension<MyDac> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class BaseExtension<TDac> : PXGraphExtension<MyGraph>
	where TDac : IBqlTable
	{
		public virtual TDac TestMethod1(TDac x, bool drilldown, double y)
		{
			return x;
		}
	}


	public class MyGraph : PXGraph<MyGraph>
	{
		public virtual void TestMethod2(Func<string> func, IEnumerable<List<string>> itemsLists)
		{
		}

		protected internal virtual object TestMethod3(Func<string> func, int x, IEnumerable<List<string>> itemsLists)
		{
			return new();
		}

		protected virtual string TestMethod4(string x)
		{
			return string.Empty;
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}