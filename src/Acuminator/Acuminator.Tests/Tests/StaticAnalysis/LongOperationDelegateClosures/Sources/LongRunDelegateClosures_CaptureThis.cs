using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		public PXAction<MyDac> testCredentials = null!;

		[PXButton]
		[PXUIField]
		public virtual IEnumerable TestCredentials(PXAdapter adapter)
		{
			var graph = PXGraph.CreateInstance<TestCredsGraph>();
			var credsAreValid = LongOperationManager.Await(ct => graph.TestCredentialAsync(this));
			return adapter.Get();
		}
	}


	public class TestCredsGraph : PXGraph<TestCredsGraph>
	{
		public Task<bool> TestCredentialAsync(ProcessingGraph graph)
		{
			return Task.FromResult(true);
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{ 
	}
}