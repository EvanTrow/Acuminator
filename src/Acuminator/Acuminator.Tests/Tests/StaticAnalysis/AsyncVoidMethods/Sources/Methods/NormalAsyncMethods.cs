using System;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethods.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public void NonAsyncVoidMethod() 
		{ }

		public async Task TaskMethodWithoutResultAsync()
		{
			await Task.Delay(1000);
		}

		public Task TaskMethodWithoutResultNonAsync() => Task.Delay(1000);

		public async Task<int> TaskMethodWithResultAsync()
		{
			await Task.Delay(1000);
			return 1;
		}

		public Task<int> TaskMethodWithResultNonAsync() => Task.FromResult(1);

		public async ValueTask<int> ValueTaskMethodWithResultAsync()
		{
			await Task.Delay(1000);
			return 1;
		}

		public async ValueTask ValueTaskMethodWithoutResultAsync()
		{
			await Task.Delay(1000);
		}
	}
}
