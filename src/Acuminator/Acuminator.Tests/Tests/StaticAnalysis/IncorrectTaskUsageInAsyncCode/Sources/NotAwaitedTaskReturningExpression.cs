using System;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public Task SimpleAsync()
		{
			// Should report diagnostic - Task-returning method not awaited
			DelayAsync();

			// Should report diagnostic - Task<T>-returning method not awaited
			GetValueAsync();

			// Should report diagnostic - ValueTask-returning method not awaited
			ProcessValueTaskAsync();

			// Should report diagnostic - ValueTask<T>-returning method not awaited  
			GetStringAsync();

			return Task.CompletedTask;
		}

		public async Task ProcessAsyncComplex()
		{
			var helper = new ExternalInstanceHelper();

			await helper.ExternalDelayAsync(DelayAsync());					 // Should report diagnostic on internal expression
			helper.ExternalOperationAsync((await GetValueAsync()));			 // Should report diagnostic on external expression
			helper.ExternalDelayAsync(DelayAsync());						 // Should report diagnostic on external expression
			await helper.ExternalOperationAsync(await GetValueAsync());		 // No diagnostic

			DelayAsync().Forget();													// Should report diagnostic
			await DelayAsync().ContinueWith(t => { }, TaskScheduler.Default);		// Should report diagnostic on first call

			ExternalStaticHelper.ExternalDelayAsync(DelayAsync());					// Should report diagnostic on both internal and external expression
			await ExternalStaticHelper.ExternalDelayAsync(DelayAsync());			// Should report diagnostic on internal expression
			ExternalStaticHelper.Forget(DelayAsync());								// Should report diagnostic
		}

		private Task DelayAsync() => Task.Delay(100);

		private Task<int> GetValueAsync() => Task.FromResult(42);

		private ValueTask ProcessValueTaskAsync() => new ValueTask();

		private ValueTask<string> GetStringAsync() => new ValueTask<string>("test");
	}
}
