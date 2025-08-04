using System;
using PX.Data;
using PX.Objects.CR;

namespace Acuminator.Tests.Sources
{
	public class OpportunityMaintCRCreateSalesOrderExt : PXGraphExtension<OpportunityMaint.CRCreateSalesOrderExt, OpportunityMaint>
	{
		public delegate void DoCreateSalesOrderDelegate();

		[PXOverride]
		public void DoCreateSalesOrder(DoCreateSalesOrderDelegate baseMethod)
		{
		}
	}
}