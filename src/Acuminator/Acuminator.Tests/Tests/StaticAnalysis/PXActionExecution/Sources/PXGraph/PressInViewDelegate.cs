using PX.Data;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXSelect<SOInvoice> Invoices;
        public PXAction<SOInvoice> Release;

        public IEnumerable invoices()
        {
            Release.Press();

            return new PXSelect<SOInvoice>(this).Select();
        }
    }

    public class SOInvoice : PXBqlTable, IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}