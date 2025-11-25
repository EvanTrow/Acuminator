using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXAction<SOInvoice> Release;
		public PXSelect<SOInvoice> MainView;

		public SOInvoiceEntry()
		{
			ExecuteRelease();
		}

		public void Initialize() =>
			ExecuteRelease();

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			ExecuteRelease();
		}

		private void ExecuteRelease()
		{
			Release.Press();
		}
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}