using PX.Data;
using PX.Async;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources.PXGraph
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SMUserMaintExt : PXGraphExtension<SMUserMaint>
	{
		[SuppressMessage("Acuminator", "PX1038:Async void lambdas, and anonymous methods are forbidden in Acumatica Framework", Justification = "<Pending>")]
		[SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
		public IEnumerable users()
		{
			PXLongOperation.StartOperation(this, () => SyncUsers());
			
			Base.LongOperationManager.StartAsyncOperation(SyncUsersAsync);
			Base.LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);

			Base.LongOperationManager.StartOperation(SyncUsers);
			Base.LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
			Base.LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
			Base.LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));

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
