using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField.Sources
{
	public class OrderMaintenanceGraph : PXGraph<OrderMaintenanceGraph>
	{
		// Regular data view - not processing
		public PXSelect<OrderDac> Orders = null!;
	}

	// Acuminator disable once PX1067 MissingBqlFieldRedeclarationInDerivedDac [Justification]
	/// <exclude/>
	[PXCacheName("Order Dac")]
	public class OrderDac : PXBqlTable, IBqlTable
	{
		#region OrderID
		public abstract class orderID : PX.Data.BQL.BqlInt.Field<orderID> { }

		[PXDBIdentity(IsKey = true)]
		public virtual int? OrderID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBString(255)]
		public virtual string? Description { get; set; }
		#endregion
	}
}