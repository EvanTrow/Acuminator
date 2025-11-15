using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly", Justification = "<Pending>")]
	public class MyGraph : PXGraph<MyGraph>
	{
		// Should report diagnostic - method returns Task but return type is object
		public object GetTaskAsObject()
		{
			return DelayAsync();
		}

		// Should report diagnostic - method returns Task<int> but return type is object
		public object GetTaskWithResultAsObject()
		{
			return GetValueAsync();
		}

		// Should report diagnostic - property returns Task but return type is object
		public object TaskProperty => ProcessValueTaskAsync();

		// Should report diagnostic - property returns Task<int> but return type is object
		public object TaskWithResultProperty => GetStringAsync();

		// Should report diagnostic - expression-bodied method returns Task but return type is void
		public void ProcessExpression() => DelayAsync();

		// Should report diagnostic - local function returns Task but return type is object
		public void UseLocalFunction()
		{
			object result = LocalFunction1();
			result		  = LocalFunction2();

			//-----------------------------------------Local Function-------------------------------------
			object LocalFunction1()
			{
				return DelayAsync();
			}

			object LocalFunction2() => GetValueAsync();
		}

		private Task DelayAsync() => Task.Delay(100);

		private Task<int> GetValueAsync() => Task.FromResult(42);

		private ValueTask ProcessValueTaskAsync() => new ValueTask();

		private ValueTask<string> GetStringAsync() => new ValueTask<string>("test");
	}
}
