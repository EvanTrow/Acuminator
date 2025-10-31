using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			// Simple "is not" pattern
			if (e.Row is not SOInvoice invoice)
				return;

			invoice.RefNbr = "<NEW>"; // should be reported

			// Recursive "is not" pattern
			if (e.Row is not { RefNbr: { Length: 2 } } invoice2)
				return;

			invoice2.RefNbr = "<NEW>"; // should be reported

			// "var" pattern
			if (e.Row is var invoice3)
				return;

			invoice3.RefNbr = "<NEW>"; // should be reported

			// "is not var" pattern, although never true, should produce a warning
			if (e.Row is not var invoice4)
				return;

			invoice4.RefNbr = "<NEW>"; // should be reported

			// parenthesized "var" pattern
			if ((e.Row, true) is var (invoice5, flag))
				return;

			invoice5.RefNbr = "<NEW>"; // should be reported

			// positional pattern
			bool flag2 = true;

			if ((e.Row, flag2) is ({ } invoice6, true) { Row.RefNbr: { } })
				return;

			invoice6.RefNbr = "<NEW>"; // should be reported

			// binary and relation patterns
			if (e.Row is not ({ RefNbr.Length: > 2 } and { RefNbr.Length: < 5 } invoice7))
				return;

			invoice7.RefNbr = "<NEW>"; // should be reported

			// Parenthesized pattern
			if (e.Row is (SOInvoice invoice8))
			{
				invoice8.RefNbr = "<NEW>"; // should be reported
				return;
			}
		}

		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			if (!(e.Row is SOInvoice invoice))
				return;

			invoice.RefNbr = "<NEW>";
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice, SOInvoice.refNbr> e)
		{
			if (!(e.Row is SOInvoice invoice))
				return;

			invoice.RefNbr = "<NEW>";
		}

		protected virtual void _(Events.RowSelected<SOLine> e)
		{
			if (!(e.Row is { } row))
				return;

			row.RefNbr = "<NEW>";
		}

		protected virtual void _(Events.FieldDefaulting<SOLine, SOLine.refNbr> e)
		{
			if (!(e.Row is { } row))
				return;

			row.RefNbr = "<NEW>";
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.refNbr> e)
		{
			if (!(e.Row is { } row))
				return;

			row.RefNbr = "<NEW>";
		}
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion
	}

	public class SOLine : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion
	}
}