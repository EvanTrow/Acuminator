using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerReturnType.Sources
{
	public class SMUserProcess : PXGraph
	{
		public PXAction<PX.SM.Users> SyncMyUsers = null!;
		public PXAction<PX.SM.Users> DontSyncYsers = null!;         // check that action delegate with slightly different name is not reported
		public PXAction<PX.SM.Users> ExpressionBodyAction = null!;
		public PXAction<PX.SM.Users> SomeAction = null!;
		public PXAction<PX.SM.Users> SomeAction1 = null!;
		public PXAction<PX.SM.Users> SomeActionWithAwait = null!;

		[PXButton]
		[PXUIField]
		public IEnumerable syncMyUsers(PXAdapter adapter)
		{
			SyncUsers();
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable dontSyncUsers(PXAdapter adapter)
		{
			yield break;
		}

		[PXButton]
		[PXUIField]
		public IEnumerable expressionBodyAction(PXAdapter adapter)
		{
			LongOperationManager.StartOperation(cToken => DoSomeWork(cToken));
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable someAction(PXAdapter adapter)
		{
			LongOperationManager.StartAsyncOperation(cToken => DoSomeWork(cToken));
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable someAction1(PXAdapter adapter)
		{
			LongOperationManager.StartAsyncOperation(async cToken => await DoSomeWork(cToken));
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable someActionWithAwait(PXAdapter adapter)
		{
			SyncUsers2(CancellationToken.None);
			return adapter.Get();
		}


		private void SyncUsers(CancellationToken cancellation = default)
		{
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Synced"));
		}

		private void SyncUsers2(CancellationToken cancellation = default)
		{
			LongOperationManager.Await(DoSomeWork, cancellation);
		}

		private Task DoSomeWork(CancellationToken cancellation = default)
		{
			return Task.Delay(100, cancellation);
		}
	}
}
