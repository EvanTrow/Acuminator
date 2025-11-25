using System;
using System.Collections.Generic;
using PX.Data;

using static PX.Objects.SO.SOOrderStatus;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingTypeListAttribute.Sources
{
	/// <exclude/> 
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class DacExtentionGood : PXCacheExtension<SOOrder>
	{
		#region Status  
		/// <summary>
		/// Gets or sets the status.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(SOOrderStatus.ListAttribute))]
		[ServiceOrderStatusExt.UpdatedList]
		public string Status { get; set; }
		#endregion		
	}

	[PXHidden]
	public partial class SOOrder : PXBqlTable, IBqlTable
	{
		#region Status
		/// <inheritdoc cref="Status"/>
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;

		/// <summary>
		/// The status of the order.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="SOOrderStatus"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[SOOrderStatus.List]
		[PXDefault]
		public virtual String Status
		{
			get {
				return this._Status;
			}
			set {
				this._Status = value;
			}
		}
		#endregion
	}

	public class SOOrderStatus 
	{
		public class ListAttribute : PXStringListAttribute
		{
			public static readonly (string, string)[] ValuesToLabels = new[]
			{
				(Open, "Open"),
				(Hold, "Hold"),
				(PendingApproval, "Balanced"),
			};

			public ListAttribute() : base(ValuesToLabels) { }
		}
	}

	public abstract class ServiceOrderStatusExt : SOOrderStatus
	{
		public class UpdatedListAttribute : PXStringListAttribute
		{
			public UpdatedListAttribute() : base(new[]
			{
				Pair(Hold, "Hold"),
				Pair(CreditHold, "Credit Hold"),
				Pair(AwaitingPayment, "Awaiting Payment")
			})
			{
			}
		}
	}
}