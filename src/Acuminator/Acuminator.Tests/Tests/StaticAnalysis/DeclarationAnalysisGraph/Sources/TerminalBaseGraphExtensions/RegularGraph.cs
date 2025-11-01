using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class RegularGraph : PXGraph<RegularGraph>
	{
		public PXSelect<SOOrder> Orders = null!;
		public PXAction<SOOrder> ProcessOrders = null!;

		protected virtual void _(Events.RowUpdated<SOOrder> e)
		{
		}
	}

	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	/// <exclude/>
	[PXCacheName("Sales Order")]
	public class SOOrder : IBqlTable
	{
		public abstract class orderNbr : BqlString.Field<orderNbr> { }

		[PXDBString(15, IsKey = true)]
		public virtual string? OrderNbr { get; set; }
	}
}