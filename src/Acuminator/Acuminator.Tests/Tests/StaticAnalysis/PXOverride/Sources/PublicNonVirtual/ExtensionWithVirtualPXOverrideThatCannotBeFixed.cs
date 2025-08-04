using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class DerivedExtension : BaseExtension
	{
		[PXOverride]
		public override void TestMethod1(int x)
		{
		}

		[PXOverride]
		public abstract void TestMethod2(Action del);
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual void TestMethod1(int x)
		{
		}
	}


	public class MyGraph : PXGraph<MyGraph>
	{
		public virtual void TestMethod1(int x)
		{	
		}

		public virtual void TestMethod2()
		{
		}
	}
}