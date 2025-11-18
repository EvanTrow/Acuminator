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

		[Inventory(DisplayName = "Component ID", IsDBField = false)]
		[PXDBCalced(typeof(Switch<Case<Where<ARTran.inventoryID, NotEqual<INTran.inventoryID>>, INTran.inventoryID>>), typeof(int))]
		public virtual int? ComponentID
		{
			get;
			set;
		}
		#endregion
	}
}
