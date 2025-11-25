using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using PX.Async;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources.PXGraph
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SMUserMaintExt : PXGraphExtension<SMUserMaint>
	{
		public const string ForceLongOperation = "_Force_";

		[SuppressMessage("Acuminator", "PX1038:Async void lambdas, and anonymous methods are forbidden in Acumatica Framework", Justification = "<Pending>")]
		public IEnumerable users([PXString] string prefix)
		{
			if (ForceLongOperation.Equals(prefix))
			{
				PXLongOperation.StartOperation(this, () => SyncUsers());

				Base.LongOperationManager.StartOperation(SyncUsers);
				Base.LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
				Base.LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
				Base.LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));
			}

			Base.LongOperationManager.StartAsyncOperation(SyncUsersAsync);
			Base.LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);

			Base.LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
			Base.LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});

			Base.LongOperationManager.StartAsyncOperation(Base, async cToken => await SyncUsersAsync(cToken));
			Base.LongOperationManager.StartAsyncOperation(Base, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});

			return new PXSelect<PX.SM.Users>(Base).Select();
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}

	public class SMUserMaint : PXGraph<SMUserMaint>
	{
		public PXSelect<PX.SM.Users> Users = null!;
	}
}
