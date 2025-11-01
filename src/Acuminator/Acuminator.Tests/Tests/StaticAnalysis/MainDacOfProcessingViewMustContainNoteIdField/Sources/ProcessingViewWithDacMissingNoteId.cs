using System;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField.Sources
{
	public class OrderProcessingGraph1 : PXGraph<OrderProcessingGraph1>
	{
		public PXFilteredProcessing<OrderDac, Filter> ProcessOrders1 = null!;

		public OrderProcessingGraph1()
		{
			ProcessOrders1.SetProcessDelegate(ProcessOrder);
		}

		private static void ProcessOrder(OrderDac order)
		{
			// Processing logic here
		}

		[PXHidden]
		public class Filter : IBqlTable
		{
			#region Type
			public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

			[PXString(4)]
			public virtual string? Type { get; set; }
			#endregion
		}
	}

	public class OrderProcessingGraph2 : PXGraph<OrderProcessingGraph2>
	{
		public PX.Data.PXProcessing<OrderDac, Where<OrderDac.type, Equal<soOrder>>> ProcessOrders2 = null!;

		public OrderProcessingGraph2()
		{
			ProcessOrders2.SetProcessDelegate(ProcessOrder);
		}

		private static void ProcessOrder(OrderDac order)
		{
			// Processing logic here
		}
	}

	/// <exclude/>
	[PXCacheName("Order DAC")]
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

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXDBString(4)]
		public virtual string? Type { get; set; }
		#endregion
	}

	public class soOrder : PX.Data.BQL.BqlString.Constant<soOrder>
	{
		public soOrder() : base("SO") { }
	}
}