using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 0)
			{
				PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
			}
		}

		public void Initialize() => 
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
		}
	}
}
