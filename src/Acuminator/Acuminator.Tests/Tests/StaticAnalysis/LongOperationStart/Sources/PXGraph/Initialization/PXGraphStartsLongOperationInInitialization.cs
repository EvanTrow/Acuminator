using System;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using static PX.Objects.CR.CRCaseCommitments.FK;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 0)
			{
				// Acuminator disable once PX1038 AsyncVoidMethodsAndLambdas [Justification]
				LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);
				LongOperationManager.StartOperation(this, cToken => SyncUsersAsync(cToken));
			}
			else
			{
				LongOperationManager.StartOperation(this, cToken => Console.WriteLine("Long Operation has been started"));
			}

			LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});
		}

		public void Initialize()
		{
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
			LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);
			LongOperationManager.StartOperation(SyncUsers);
			LongOperationManager.StartAsyncOperation(SyncUsersAsync);

			LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
			LongOperationManager.StartAsyncOperation(this, SyncUsersAsync);
			LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
			LongOperationManager.StartAsyncOperation(this, async cToken => await SyncUsersAsync(cToken));
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
