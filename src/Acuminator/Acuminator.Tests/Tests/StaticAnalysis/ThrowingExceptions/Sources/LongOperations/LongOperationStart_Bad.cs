using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.LongOperations
{
	// Acuminator disable once PX1018 NoPrimaryViewForPrimaryDac [Justification]
	[SuppressMessage("Acuminator", "PX1050:Hardcoded strings are not allowed as parameters for localization methods and PXException constructors." +
					 " You should use string constants from the appropriate localizable messages class.", Justification = "<Pending>")]
	[SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
	public class UserMaint : PXGraph<UserMaint, Users>
	{
		public PXAction<Users> LongrunAction = null!;

		[PXButton, PXUIField]
		[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>")]
		public IEnumerable longrunAction(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(UID, BackgroundOperation);
			PXLongOperation.StartOperation(UID, () => throw new PXSetupNotEnteredException<Users>(null));

			LongOperationManager.StartAsyncOperation(SyncUsersAsync);
			LongOperationManager.StartAsyncOperation(this, cToken => throw new PXSetupNotEnteredException<Users>(null));

			LongOperationManager.StartOperation(SyncUsers);
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
			// Acuminator disable once PX1038 AsyncVoidMethodsAndLambdas [Justification]
			LongOperationManager.StartOperation(this, async cToken => await SyncUsersAsync(cToken));
			LongOperationManager.StartOperation(this, cToken => throw new PXSetupNotEnteredException<Users>(null));

			LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
			LongOperationManager.Await(delegate (CancellationToken cToken)
			{
				SyncUsers(cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});

			LongOperationManager.StartAsyncOperation(this, async cToken => await SyncUsersAsync(cToken));
			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});

			LongOperationManager.StartAsyncOperation(this, async delegate (CancellationToken cToken)
			{
				await SyncUsersAsync(cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});
			return adapter.Get();
		}

		public static void BackgroundOperation()
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		public static Task SyncUsersAsync(CancellationToken cancellation = default) =>
			throw new PXSetupNotEnteredException<Users>(null);
	}
}
