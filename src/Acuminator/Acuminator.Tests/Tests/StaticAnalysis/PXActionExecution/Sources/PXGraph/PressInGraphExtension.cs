using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		public PXAction<SOInvoice> CustomRelease;

		public SOInvoiceEntryExt()
		{
			CustomRelease.Press();
		}

		public override void Initialize() =>
			Base.Release.Press();

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			Base.Release.Press();
		}
	}

	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXAction<SOInvoice> Release;
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}