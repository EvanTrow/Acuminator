using System;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		private readonly Task _taskField1 = DelayAsync();
		private readonly Task _taskField2, _taskField3;

		public async Task ProcessAsync()
		{
			// Should report diagnostic - storing Task in variable
			Task task = DelayAsync();
			await task;

			// Should report diagnostic - storing Task<T> in variable
			Task<int> taskWithResult = GetValueAsync();
			int result = await taskWithResult;

			// Should report diagnostic - storing ValueTask in variable
			ValueTask valueTask = ProcessValueTaskAsync(), valueTask1 = ProcessValueTaskAsync();
			await valueTask;

			// Should report diagnostic - storing ValueTask<T> in variable
			ValueTask<string> valueTaskWithResult = GetStringAsync();
			string text = await valueTaskWithResult;

			Task task1;
		}

		private static Task DelayAsync() => Task.Delay(100);

		private Task<int> GetValueAsync() => Task.FromResult(42);

		private ValueTask ProcessValueTaskAsync() => new ValueTask();

		private ValueTask<string> GetStringAsync() => new ValueTask<string>("test");
	}
}
