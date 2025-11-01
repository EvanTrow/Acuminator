using PX.Data;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public PXSelect<SOInvoice> Invoices;

        public IEnumerable invoices()
        {
            Base.Release.Press();

            return new PXSelect<SOInvoice>(Base).Select();
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry>
    {
        public PXAction<SOInvoice> Release;
    }

    public class SOInvoice : PXBqlTable, IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}