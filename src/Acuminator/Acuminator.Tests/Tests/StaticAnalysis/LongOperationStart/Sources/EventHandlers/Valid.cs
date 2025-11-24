using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using PX.Async;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources
{
	// Acuminator disable once PX1018 NoPrimaryViewForPrimaryDac [Justification]
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXAction<SOInvoice> Release;

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			e.Cancel = PXLongOperation.Exists(this.UID);

			var info = LongOperationManager.GetCustomInfoFor(UID, "Key");
			var status = LongOperationManager.GetStatus();
			LongOperationManager.ClearStatus();
		}

		[PXButton]
		[PXUIField]
		[SuppressMessage("Acuminator", "PX1038:Async void lambdas, and anonymous methods are forbidden in Acumatica Framework", Justification = "<Pending>")]
		protected virtual IEnumerable release(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(SyncUsers);
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
			LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
			LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));

			return adapter.Get();
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}

	[PXHidden]
	public class SOInvoice : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(8, IsKey = true, InputMask = "")]
		public string? RefNbr { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string? CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string? LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}