using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		public SOInvoiceEntryExt()
		{
			Base.Release.Press(null);
		}

		public override void Initialize() =>
			Base.Release.Press(null);

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			Base.Release.Press(null);
		}
	}

	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXAction<SOInvoice> Release;

		public PXSelect<SOInvoice> MainView;
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}