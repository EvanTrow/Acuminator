using System;
using System.Threading;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources
{
	// Acuminator disable once PX1018 NoPrimaryViewForPrimaryDac [Justification]
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.FieldDefaulting<SOInvoice.refNbr> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			StartOperation();
			StartOperation1();
			StartOperation2();
			StartOperation3();
			StartOperation4();
		}

		private void StartOperation()
		{
			PXLongOperation.StartOperation(this, null);
		}

		private void StartOperation1()
		{
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
		}

		private void StartOperation2()
		{
			LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});
		}

		private void StartOperation3()
		{
			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});
		}

		private void StartOperation4() =>
			LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);

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
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
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