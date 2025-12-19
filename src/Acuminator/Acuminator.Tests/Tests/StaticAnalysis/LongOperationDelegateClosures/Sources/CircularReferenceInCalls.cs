using PX.Async;
using PX.Data;

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
	[PXHidden]
	public class SomeDAC : PXBqlTable, IBqlTable { }

	public class SomeGraph : PXGraph<SomeGraph>
	{
		PXAction<SomeDAC> SomeAction = null!;

		public IEnumerable someAction(PXAdapter adapter)
		{
			RecursiveCallWithLocalFunction(recursionDepth: 0);
			RecursiveCall(recursionDepth: 0);
			return adapter.Get();
		}

		private void RecursiveCallWithLocalFunction(int recursionDepth)
		{
			if (recursionDepth > 100)
				return;

			LocalFunction(recursionDepth);

			//--------------------------------------------Local Function-----------------------------------------------
			void LocalFunction(int recursionDepth)
			{
				RecursiveCallWithLocalFunction(recursionDepth + 1);
			}
		}

		private void RecursiveCall(int recursionDepth)
		{
			if (recursionDepth > 100)
				return;

			RecursiveCall(recursionDepth + 1);
		}
	}
}