using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;

using static PX.Objects.CR.CRCaseCommitments.FK;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources.PXGraph
{
	public class SMUserMaint : PX.Data.PXGraph<SMUserMaint>
	{
		public PXSelect<PX.SM.Users> Users;

		public IEnumerable users()
		{
			SomeMethod();
			SomeMethod1();
			SomeMethod2();
			SomeMethod3();
			SomeMethod4();

			return new PXSelect<PX.SM.Users>(this).Select();
		}

		private void SomeMethod()
		{
			PXLongOperation.StartOperation(this, () => SyncUsers());
		}

		private void SomeMethod1()
		{
			LongOperationManager.StartOperation(this, cToken => SyncUsers(cToken));
		}

		private void SomeMethod2()
		{
			LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});
		}

		private void SomeMethod3()
		{
			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});
		}

		private void SomeMethod4() =>
			LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}
}
