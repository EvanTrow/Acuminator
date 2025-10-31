using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		public SOInvoiceEntryExt()
		{
			ExecuteRelease();
		}

		public override void Initialize()
		{
			ExecuteRelease();
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			ExecuteRelease();
		}

		private void ExecuteRelease()
		{
			Base.Release.Press();
		}
	}

	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXAction<SOInvoice> Release;
	}

	[PXHidden]
	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}