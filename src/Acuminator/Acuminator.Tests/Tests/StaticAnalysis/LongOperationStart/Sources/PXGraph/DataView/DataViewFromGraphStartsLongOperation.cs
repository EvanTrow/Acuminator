using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources.PXGraph
{
	public class SMUserMaint : PX.Data.PXGraph<SMUserMaint>
	{
		public PXSelect<PX.SM.Users> Users = null!;

		[SuppressMessage("Acuminator", "PX1038:Async void lambdas, and anonymous methods are forbidden in Acumatica Framework", Justification = "<Pending>")]
		[SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
		public IEnumerable users()
		{
			PXLongOperation.StartOperation(this, () => SyncUsers());

			LongOperationManager.StartOperation(SyncUsers);
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
			LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
			LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));

			LongOperationManager.StartAsyncOperation(SyncUsersAsync);
			LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);

			LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
			LongOperationManager.Await(cToken =>
										{
											SyncUsers(cToken);
											return Task.CompletedTask;
										});

			LongOperationManager.StartAsyncOperation(this, async cToken => await SyncUsersAsync(cToken));
			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});

			return new PXSelect<PX.SM.Users>(this).Select();
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}
}
