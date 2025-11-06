using PX.Data;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerReturnType.Sources
{
	public class SMUserProcess : PXGraph
	{
		public PXAction<PX.SM.Users> SyncMyUsers = null!;
		public PXAction<PX.SM.Users> DontSyncYsers = null!;         // check that action delegate with typo is not reported

		[PXButton]
		[PXUIField]
		public void syncMyUsers()
		{
			SyncUsers();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable dontSyncUsers()
		{
			yield return null;
		}

		private void SyncUsers()
		{
			PXLongOperation.StartOperation(this, () => Console.WriteLine("Synced"));
		}
	}
}
