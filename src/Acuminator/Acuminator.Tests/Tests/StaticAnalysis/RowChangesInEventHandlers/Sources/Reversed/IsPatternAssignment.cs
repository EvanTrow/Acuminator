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
		public PXSelect<SOInvoice> Documents;

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			// Simple "is not" pattern
			if (e.Row is not SOInvoice invoice)
				return;

			invoice.RefNbr = "<NEW>"; // should be OK

			// Recursive "is not" pattern
			if (e.Row is not { RefNbr: { Length: 2 } } invoice2)
				return;

			invoice2.RefNbr = "<NEW>"; // should be OK

			// "var" pattern
			if (e.Row is var invoice3)
				return;

			invoice3.RefNbr = "<NEW>"; // should be OK

			// "is not var" pattern, although never true, should not produce a warning
			if (e.Row is not var invoice4)
				return;

			invoice4.RefNbr = "<NEW>"; // should be OK

			// parenthesized "var" pattern
			if ((e.Row, true) is var (invoice5, flag))
				return;

			invoice5.RefNbr = "<NEW>"; // should be OK

			// positional pattern
			bool flag2 = true;

			if ((e.Row, flag2) is ({ } invoice6, true) { Row.RefNbr: { } })
				return;

			invoice6.RefNbr = "<NEW>"; // should be OK

			// binary and relation patterns
			if (e.Row is not ({ RefNbr.Length: > 2 } and { RefNbr.Length: < 5 } invoice7))
				return;

			invoice7.RefNbr = "<NEW>"; // should be OK

			// Parenthesized pattern
			if (e.Row is (SOInvoice invoice8))
			{
				invoice8.RefNbr = "<NEW>"; // should be OK
				return;
			}
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			if (!(e.Row is SOInvoice invoice))
				return;

			invoice.RefNbr = "<NEW>"; // should be OK
		}

		protected virtual void _(Events.RowInserting<SOLine> e)
		{
			if (!(e.Row is { } row))
				return;

			row.RefNbr = "<NEW>"; // should be OK
		}

		protected virtual void _(Events.RowSelecting<SOLine> e)
		{
			if (!(e.Row is { RefNbr: { } } row))
				return;

			row.RefNbr = "<NEW>"; // should be OK
		}

		protected virtual void _(Events.FieldUpdating<SOInvoice.refNbr> e)
		{
			if (e.Row is not SOInvoice row)
				return;

			row.RefNbr = "<NEW>"; // should be OK
		}
	}

	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion
	}

	public class SOLine : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion
	}
}