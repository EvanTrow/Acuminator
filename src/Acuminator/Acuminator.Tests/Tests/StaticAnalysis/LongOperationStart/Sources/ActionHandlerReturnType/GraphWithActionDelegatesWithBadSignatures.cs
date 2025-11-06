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
		public void syncMyUsers()
		{
			SyncUsers();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable dontSyncUsers(PXAdapter adapter)
		{
			yield break;
		}

		[PXButton]
		[PXUIField]
		public void expressionBodyAction() =>
			LongOperationManager.StartOperation(cToken => DoSomeWork(cToken));

		[PXButton]
		[PXUIField]
		public void someAction()
		{
			LongOperationManager.StartAsyncOperation(cToken => DoSomeWork(cToken));
		}

		[PXButton]
		[PXUIField]
		public void someAction1()
		{
			LongOperationManager.StartAsyncOperation(async cToken => await DoSomeWork(cToken));
		}

		[PXButton]
		[PXUIField]
		public void someActionWithAwait()
		{
			SyncUsers2(CancellationToken.None);
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
