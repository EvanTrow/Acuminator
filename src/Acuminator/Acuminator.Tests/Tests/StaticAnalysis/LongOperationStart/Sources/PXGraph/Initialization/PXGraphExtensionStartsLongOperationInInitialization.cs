using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SMAccessExt : PXGraphExtension<SMAccessPersonalMaint>
	{
		public SMAccessExt()
		{
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
		}

		public override void Initialize()
		{
			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
			}
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
		}
	}
}
