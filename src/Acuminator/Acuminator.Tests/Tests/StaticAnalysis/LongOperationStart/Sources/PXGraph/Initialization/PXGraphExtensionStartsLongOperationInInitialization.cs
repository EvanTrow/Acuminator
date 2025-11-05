using System;
using System.Threading;
using System.Threading.Tasks;

using PX.Async;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class SMAccessExt : PXGraphExtension<SomeGraph>
	{
		public static bool IsActive()
		{
			PXLongOperation.StartOperation(null, () => Console.WriteLine("Long Operation has been started"));
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		public static bool IsActiveForGraph<TGraph>()
		{
			PXLongOperation.StartOperation(null, () => Console.WriteLine("Long Operation has been started"));
			return typeof(TGraph) == typeof(SomeGraph);
		}

		// Acuminator disable once PX1040 ConstructorInGraphExtension [Justification]
		public SMAccessExt()
		{
			PXLongOperation.StartOperation(null, () => Console.WriteLine("Long Operation has been started"));
			Base.LongOperationManager.StartOperation(SyncUsers);
			Base.LongOperationManager.StartAsyncOperation(SyncUsersAsync);

			Base.LongOperationManager.Await(cToken =>
			{
				SyncUsers(cToken);
				return Task.CompletedTask;
			});
		}

		public override void Initialize()
		{
			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				// Acuminator disable once PX1038 AsyncVoidMethodsAndLambdas [Justification]
				Base.LongOperationManager.StartAsyncOperation(Base, SyncUsersAsync);
				Base.LongOperationManager.StartOperation(Base, cToken => SyncUsersAsync(cToken));
			}
			else
			{
				Base.LongOperationManager.StartOperation(Base, cToken => Console.WriteLine("Long Operation has been started"));
			}

			Base.LongOperationManager.StartAsyncOperation(this, async cToken =>
			{
				await SyncUsersAsync(cToken);
				await Task.Delay(5, cToken);
			});
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
			Base.LongOperationManager.StartAsyncOperation(Base, SyncUsersAsync);
			Base.LongOperationManager.Await(SyncUsersAsync, CancellationToken.None);
			Base.LongOperationManager.StartAsyncOperation(this, async cToken => await SyncUsersAsync(cToken));
		}

		public static void SyncUsers(CancellationToken cancellation = default)
		{
		}

		public static async Task SyncUsersAsync(CancellationToken cancellation = default)
		{
			await Task.Yield();
		}
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<Users> Identities = null!;
	}
}
