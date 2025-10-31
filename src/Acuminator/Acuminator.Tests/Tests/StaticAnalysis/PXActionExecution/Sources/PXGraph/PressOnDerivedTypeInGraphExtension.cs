using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		public SOInvoiceEntryExt()
		{
			Base.Cancel.Press();
		}

		public override void Initialize() =>
			Base.Cancel.Press();

		public override void Configure(PXScreenConfiguration configuration)
		{
			Base.Cancel.Press();
			base.Configure(configuration);
		}
	}

	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXSelect<SOInvoice> MainView;
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}