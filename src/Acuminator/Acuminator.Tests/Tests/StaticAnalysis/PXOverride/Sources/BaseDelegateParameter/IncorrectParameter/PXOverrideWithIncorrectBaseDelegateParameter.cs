using System;
using System.Collections.Generic;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedExtension : PXGraphExtension<DerivedExtension, MyGraph>
	{
		public delegate string CustomDelegateType(string input);

		[PXOverride]
		public MyDac TestMethod1(MyDac x, bool drilldown, double y, Func<MyDac, bool, MyDac> base_TestMethod1) => x;

		[PXOverride]
		public void TestMethod2(Func<string> func, IEnumerable<List<string>> itemsLists, 
								Action<Func<string>, IEnumerable<List<DateTime>>> base_TestMethod2)
		{	
		}

		[PXOverride]
		public object TestMethod3(Func<string> func, int x, IEnumerable<List<string>> itemsLists, 
								 Func<Func<string>, IEnumerable<List<object>>, string> base_TestMethod3)
		{
			return new();
		}

		[PXOverride]
		public void TestMethod4(CustomDelegateType base_TestMethod4)
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

		protected virtual void TestMethod4()
		{
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}