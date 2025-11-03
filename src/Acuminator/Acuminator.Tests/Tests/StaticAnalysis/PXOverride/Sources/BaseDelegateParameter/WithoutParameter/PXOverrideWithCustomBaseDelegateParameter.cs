using System;
using System.Collections.Generic;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedExtension : PXGraphExtension<DerivedExtension, MyGraph>
	{
		public delegate MyDac CustomBaseDelegate1(MyDac x, bool drilldown, double y);
		public delegate void CustomBaseDelegate2(Func<string> func, IEnumerable<List<string>> itemsLists);
		public delegate object CustomBaseDelegate3(Func<string> func, int x, IEnumerable<List<string>> itemsLists);

		[PXOverride]
		public MyDac TestMethod1(MyDac x, bool drilldown, double y, CustomBaseDelegate1 base_TestMethod1) => x;

		[PXOverride]
		public void TestMethod2(Func<string> func, IEnumerable<List<string>> itemsLists, CustomBaseDelegate2 base_TestMethod2)
		{
		}

		[PXOverride]
		public object TestMethod3(Func<string> func, int x, IEnumerable<List<string>> itemsLists, CustomBaseDelegate3 base_TestMethod3)
		{
			return new();
		}

		[PXOverride]
		public void TestMethod4(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected base_TestMethod4)
		{ }
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedExtension : BaseExtension<MyDac> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class BaseExtension<TDac> : PXGraphExtension<MyGraph>
	where TDac : PXBqlTable, IBqlTable
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

		protected virtual void TestMethod4(PXCache cache, PXRowSelectedEventArgs e)
		{
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}