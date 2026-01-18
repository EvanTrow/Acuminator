using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APReleaseProcessDatasourceExt : PXGraphExtension<MyGraph>
	{
		public delegate void DoSomething1DelegateType(ref MyDac doc, bool isPrebooking, List<MyDac> inDocs);

		[PXOverride]
		public void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething1DelegateType base_DoSomething1)
		{
			inDocs = new List<MyDac>();
			base_DoSomething1(ref doc, isPrebooking, inDocs);
		}

		public delegate void DoSomething2DelegateType(ref MyDac doc, in bool isPrebooking, out List<MyDac> inDocs);

		[PXOverride]
		public void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething2DelegateType base_DoSomething2)
		{
			inDocs = new List<MyDac>();
			base_DoSomething2(ref doc, isPrebooking, out inDocs);
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		protected virtual void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}

		protected virtual void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}