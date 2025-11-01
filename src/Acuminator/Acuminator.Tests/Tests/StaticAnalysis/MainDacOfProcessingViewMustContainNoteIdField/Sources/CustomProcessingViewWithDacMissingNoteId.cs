using System;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField.Sources
{
	public class OrderProcessingGraph : PXGraph<OrderProcessingGraph>
	{
		public class CustomProcessingView : PXProcessing<OrderDac>
		{
			public CustomProcessingView(PXGraph graph) : base(graph) { }

			public CustomProcessingView(PXGraph graph, Delegate handler) : base(graph, handler) { }
		}

		public CustomProcessingView ProcessOrders = null!;

		public OrderProcessingGraph()
		{
			ProcessOrders.SetProcessDelegate(ProcessOrder);
		}

		private static void ProcessOrder(OrderDac order)
		{
			// Processing logic here
		}
	}

	/// <exclude/>
	[PXCacheName("Order DAC")]
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

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(4)]
		public virtual string? Type { get; set; }
		#endregion
	}
}