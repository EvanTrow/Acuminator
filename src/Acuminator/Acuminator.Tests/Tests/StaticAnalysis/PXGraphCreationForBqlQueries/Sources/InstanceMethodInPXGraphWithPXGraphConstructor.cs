using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationForBqlQueries.Sources
{
	public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
	{
		public ARInvoice GetInvoice(string refNbr)
		{
			var invoice = PXSelect<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>
				.Select(new PXGraph(), "000001");

			return invoice;
		}

		public ARInvoice GetInvoice2(string refNbr)
		{
			var invoice = PXSelect<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>
				.Select(new(), "000001");

			return invoice;
		}
	}

	[PXHidden]
	public class ARInvoice : PXBqlTable, IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : IBqlField { }
		
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string? RefNbr { get; set; }
		#endregion
	}
}