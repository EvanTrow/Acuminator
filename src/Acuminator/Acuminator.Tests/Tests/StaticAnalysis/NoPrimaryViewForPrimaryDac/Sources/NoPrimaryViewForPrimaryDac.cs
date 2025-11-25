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

	public class SOTran : PXBqlTable, IBqlTable { }

	public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
	{
		public PXSelect<ARTran> Details;

	}

	public class SOInvoiceEntry : ARInvoiceEntry
	{
		public PXSelect<SOTran> SODetails;
	}
}
