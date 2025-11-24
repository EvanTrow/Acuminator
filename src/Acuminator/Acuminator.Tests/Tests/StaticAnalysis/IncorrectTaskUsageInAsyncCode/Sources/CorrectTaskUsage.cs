using System;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public async Task ProcessAsync()
		{
			// Correct - awaiting Task directly without storing
			await DelayAsync();

			// Correct - awaiting ValueTask directly without storing
			await ProcessValueTaskAsync();
		}

		// Correct - method returns Task
		public Task GetTaskAsync()
		{
			return LocalFunc();

			//-------------------------------Local Function--------------------------
			Task LocalFunc()
			{
				return DelayAsync();
			}
		}

		// Correct - method returns Task<int>
		public async Task<string> GetTaskWithResultAsync()
		{
			string res = await LocalFunc();
			return res;

			//-------------------------------Local Function--------------------------
			ValueTask<string> LocalFunc() => GetStringAsync();
		}

		// Correct - expression-bodied method returns Task
		public Task ProcessExpression() => DelayAsync();

		// Correct - property returns Task
		public Task TaskProperty => DelayAsync();

		private Task DelayAsync() => Task.Delay(100);

		private Task<int> GetValueAsync() => Task.FromResult(42);

		private Task<int> GetExternalValueAsync() => new Helper().GetValueAsync();

		private ValueTask RunExternalTaskASync()
		{
			return Helper.ProcessValueTaskAsync();
		}

		private ValueTask ProcessValueTaskAsync() => new ValueTask();

		private ValueTask<string> GetStringAsync() => new ValueTask<string>("test");

		private Task RunExternalTaskAsync2() => Helper.ProcessTaskAsync();
	}


	public class Helper
	{
		public Task<int> GetValueAsync() => Task.FromResult(42);

		public static Task ProcessTaskAsync() => Task.CompletedTask;

		public static ValueTask ProcessValueTaskAsync() => new ValueTask();
	}
}
