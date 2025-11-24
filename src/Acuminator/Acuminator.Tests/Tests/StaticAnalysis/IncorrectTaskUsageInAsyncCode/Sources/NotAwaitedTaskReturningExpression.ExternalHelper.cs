using System;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public static class ExternalStaticHelper
	{
		public static async Task ExternalDelayAsync(this Task task)
		{
			await Task.Delay(100);
			await task;
		}

		// Acuminator disable once PX1038 AsyncVoidMethodsAndLambdas [Justification]
		public static async void Forget(this Task task)
		{
			try
			{
				await task;
			}
			catch (Exception)
			{
			}
		}
	}

	public class ExternalInstanceHelper
	{
		public async Task ExternalDelayAsync(Task task)
		{
			await Task.Delay(100);
			await task;
		}

		public async Task<int> ExternalOperationAsync(int value)
		{
			await Task.Delay(100);
			return value;
		}
	}
}
