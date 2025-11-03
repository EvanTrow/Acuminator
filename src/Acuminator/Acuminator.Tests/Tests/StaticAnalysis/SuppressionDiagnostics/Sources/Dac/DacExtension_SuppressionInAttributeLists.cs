using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.GL;
using PX.Objects.PM;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>
	public sealed class SOOrderExt : PXCacheExtension<SOOrder>
	{
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[OpenPeriod(null, typeof(PMTran.date),
					branchSourceType: typeof(PMTran.branchID),
					masterFinPeriodIDType: typeof(PMTran.tranPeriodID),
					redefaultOrRevalidateOnOrganizationSourceUpdated: false, IsDBField = false)]
		[PXDefault]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public string? FinPeriodID { get; set; }
		#endregion
	}


	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class SOOrder : PXBqlTable, IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string? OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string? OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		[PXString]
		public string? Status { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[]? tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
