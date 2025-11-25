using System;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
	public class MyGraph : PXGraph<MyGraph>
	{
		
		public void Process()
		{
			LongOperationManager.StartAsyncOperation(ct => ProcessMobilePaymentAsync());

			LongOperationManager.StartAsyncOperation(ProcessMobilePaymentWithCancellationAsync);

			LongOperationManager.Await(cancellationToken => GetHelper().PreparePaymentFormAsync());
			LongOperationManager.Await(async cancellationToken => await GetHelper().PreparePaymentFormAsync());
			

			LongOperationManager.StartAsyncOperation(ct =>
			{
				var graph = PXGraph.CreateInstance<MyGraph>();
				return graph.AuthorizeWithTerminalAsync();
			});

			LongOperationManager.StartAsyncOperation(delegate (CancellationToken ct)
			{
				var graph = PXGraph.CreateInstance<MyGraph>();
				return graph.AuthorizeWithTerminalAsync();
			});
		}

		
		private static Task ProcessMobilePaymentAsync()
		{
			return Task.CompletedTask;
		}

		private static Task ProcessMobilePaymentWithCancellationAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public virtual Task AuthorizeWithTerminalAsync() => Task.CompletedTask;


		private static Helper GetHelper() => new Helper();
	}

	public class Helper
	{
		public virtual Task PreparePaymentFormAsync() => Task.CompletedTask;
	}
}
