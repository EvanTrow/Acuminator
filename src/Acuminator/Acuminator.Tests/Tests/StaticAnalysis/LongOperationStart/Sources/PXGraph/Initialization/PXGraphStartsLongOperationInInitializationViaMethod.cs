using System;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users = null!;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 0)
			{
				StartLongOperation();
				SomeMethod1();
				SomeMethod2();
			}

			SomeMethod3();
			SomeMethod4();
		}

		public void Initialize()
		{
			StartLongOperation();
			SomeMethod1();
			SomeMethod2();
			SomeMethod3();
			SomeMethod4();
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);

			StartLongOperation();
			SomeMethod1();
			SomeMethod2();
			SomeMethod3();
			SomeMethod4();
		}



		private void StartLongOperation()
		{
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
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
