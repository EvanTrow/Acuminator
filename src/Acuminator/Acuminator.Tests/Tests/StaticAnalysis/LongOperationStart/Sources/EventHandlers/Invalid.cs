using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;

using static PX.Objects.CR.CRCaseCommitments.FK;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources
{
	// Acuminator disable once PX1018 NoPrimaryViewForPrimaryDac [Justification]
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.FieldDefaulting<SOInvoice.refNbr> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(SyncUsers);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			// Acuminator disable once PX1038 AsyncVoidMethodsAndLambdas [Justification]
			LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartAsyncOperation(this, async cToken => await SyncUsersAsync(cToken));
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(SyncUsers);
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartAsyncOperation(SyncUsersAsync);
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
			LongOperationManager.StartAsyncOperation(SyncUsersAsync);
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}

	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class SOInvoice : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}
}