using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXAction<SOInvoice> Release;

		public SOInvoiceEntry()
		{
			Release.Press();
		}

		public void Initialize() => 
			Release.Press();

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			Release.Press();
		}
	}

	public class SOInvoice : IBqlTable
	{
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
	}
}