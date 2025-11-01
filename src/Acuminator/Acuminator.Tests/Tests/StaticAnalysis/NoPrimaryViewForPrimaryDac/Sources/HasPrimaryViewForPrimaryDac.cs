using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class ARInvoice : PXBqlTable, IBqlTable { }

	public class ARTran : PXBqlTable, IBqlTable { }

	public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
	{
		public PXSelect<ARInvoice> Document;

		public PXSelect<ARTran> Details;
	}
}
