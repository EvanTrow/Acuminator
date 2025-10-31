using PX.Data;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>, IGraphWithInitialization
	{
		public SOInvoiceEntry()
		{
			Cancel.Press();
		}

		public void Initialize() => Cancel.Press();

		public override void Configure(PXScreenConfiguration graph)
		{
			Cancel.Press();
			base.Configure(graph);
		}
	}

	[PXHidden]
	public class SOInvoice : PXBqlTable, IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}