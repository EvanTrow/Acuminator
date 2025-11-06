using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.LongOperations
{
	// Acuminator disable once PX1018 NoPrimaryViewForPrimaryDac [Justification]
	public class UserMaint : PXGraph<UserMaint, Users>
	{
		public PXAction<Users> LongrunAction = null!;
		public PXAction<Users> LongrunAction1 = null!;

		[PXButton]
		[PXUIField]
		public IEnumerable longrunAction(PXAdapter adapter)
		{
			object uid = UID;
			PXLongOperation.StartOperation(UID, BackgroundOperation1);
			PXLongOperation.StartOperation(UID, () => BackgroundOperation2(uid));
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable longrunAction1(PXAdapter adapter)
		{
			object uid = UID;

			LongOperationManager.StartOperation(cToken => BackgroundOperation1());
			LongOperationManager.StartAsyncOperation(cToken => throw new PXException());
			BackgroundOperation3();

			return adapter.Get();
		}

		public static void BackgroundOperation1()
		{
			throw new PXException();
		}

		public static void BackgroundOperation2(object uid)
		{
			PXLongOperation.StartOperation(uid, BackgroundOperation1);
		}

		public void BackgroundOperation3()
		{
			LongOperationManager.StartAsyncOperation(UID, cToken =>
			{
				throw new PXException();
			});
			LongOperationManager.Await(cToken => throw new PXException());
		}
	}
}
