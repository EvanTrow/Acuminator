using System;

using PX.Data;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacFieldWithDBCalcedAttribute.Sources
{
	/// <exclude/>
	[PXCacheName("Invoice Split")]
	[InvoiceSplitProjection(true)]
	public class InvoiceSplit : PXBqlTable, IBqlTable
	{
		#region ComponentID
		public abstract class componentID : BqlInt.Field<componentID> { }

		// Entity Attribute has IsDBField set directly on the property
		[Inventory(DisplayName = "Component ID", IsDBField = false)]
		[PXDBCalced(typeof(Switch<Case<Where<ARTran.inventoryID, NotEqual<INTran.inventoryID>>, INTran.inventoryID>>), typeof(int))]
		public virtual int? ComponentID
		{
			get;
			set;
		}
		#endregion

		#region ComponentID1
		public abstract class componentID1 : BqlInt.Field<componentID1> { }

		// Entity Attribute has IsDBField set directly on the property
		[MyEntityAttribute]
		[PXDBCalced(typeof(Switch<Case<Where<ARTran.inventoryID, NotEqual<INTran.inventoryID>>, INTran.inventoryID>>), typeof(int))]
		public virtual int? ComponentID1
		{
			get;
			set;
		}
		#endregion
	}


	[Inventory(DisplayName = "Component ID", IsDBField = false)]
	public class MyEntityAttribute : PXAggregateAttribute 
	{

	}
}
