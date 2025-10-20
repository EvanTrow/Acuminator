using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField.Sources
{
	public class OrderProcessingGraph : PXGraph<OrderProcessingGraph>
	{
		public PXProcessing<OrderDac> OrdersToProcess = null!;

		public OrderProcessingGraph()
		{
			OrdersToProcess.SetProcessDelegate<OrderProcessingGraph>(ProcessOrder);
		}

		private static void ProcessOrder(OrderProcessingGraph graph, OrderDac order)
		{
			// Processing logic here
		}
	}

	// Acuminator disable once PX1067 MissingBqlFieldRedeclarationInDerivedDac [Justification]
	/// <exclude/>
	[PXCacheName("Order Dac")]
	public class OrderDac : IBqlTable
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

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}