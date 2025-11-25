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
		PXProcessing<SomeDAC> _processing = null!;

		PXAction<SomeDAC> SomeAction = null!;

		public IEnumerable someAction(PXAdapter adapter)
		{
			var helper = new NonGraph();
			helper.RunLongRunWithoutCapture(_processing);						// No diagnostic
			helper.CaptureInLongRunArray(this, this);                           // Show diagnostic
			helper.CaptureInLongRun(this);										// Show diagnostic
			helper.SetProcessingWithGraphCapture(_processing, this);			// Show diagnostic
			helper.SetProcessingWithAdapterCapture(_processing, adapter);		// Show diagnostic

			helper.CaptureInLongRun_LongOperationManager_StartOperation(LongOperationManager, this);			 // Show diagnostic
			helper.CaptureInLongRun_LongOperationManager_StartAsyncOperation(LongOperationManager, this);		 // Show diagnostic
			helper.CaptureInLongRun_LongOperationManager_Await(LongOperationManager, adapter);					 // Show diagnostic
			helper.CaptureInLongRun_IGraphLongOperationManager_StartOperation(LongOperationManager, this);		 // Show diagnostic
			helper.CaptureInLongRun_IGraphLongOperationManager_StartAsyncOperation(LongOperationManager, this);  // Show diagnostic

			return adapter.Get();
		}
	}


	public class NonGraph
	{
		private static readonly Guid ID = Guid.NewGuid();

		public virtual void RunLongRunWithoutCapture(PXProcessing<SomeDAC> processingView)
		{
			processingView.SetProcessDelegate((SomeDAC item) => MemberFunc());      // No diagnostic

			PXLongOperation.StartOperation(ID, () => MemberFunc());					// No diagnostic
		}

		public virtual void SetProcessingWithGraphCapture(PXProcessing<SomeDAC> processingView, PXGraph graph)
		{
			processingView.SetProcessDelegate((SomeDAC item) =>                         // No diagnostic
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				} 		
			});
		}

		public virtual void SetProcessingWithAdapterCapture(PXProcessing<SomeDAC> processingView, PXAdapter adapterToCapture)
		{
			processingView.SetProcessDelegate((SomeDAC item) =>                         // No diagnostic
			{
				if (adapterToCapture != null)
					MemberFunc();
			});
		}

		public void CaptureInLongRun(PXGraph graph)
		{
			PXLongOperation.StartOperation(graph, () =>
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				}
			});
		}

		public void CaptureInLongRunArray(PXGraph graph, params object[] args)
		{
			PXLongOperation.StartOperation(graph, () =>
			{
				if (args[0] != null)
				{
					MemberFunc();
				}
			});
		}

		public void CaptureInLongRun_LongOperationManager_StartOperation(ILongOperationManager longOperationManager, PXGraph graph)
		{
			longOperationManager.StartOperation(graph, cToken =>
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				}
			});
		}

		public void CaptureInLongRun_LongOperationManager_StartAsyncOperation(ILongOperationManager longOperationManager, PXGraph graph)
		{
			longOperationManager.StartAsyncOperation(ID, cToken =>
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				}

				return Task.CompletedTask;
			});
		}

		public void CaptureInLongRun_LongOperationManager_Await(ILongOperationManager longOperationManager, object adapter)
		{
			longOperationManager.Await(cToken =>
			{
				if (adapter != null)
				{
					MemberFunc();
				}

				return Task.CompletedTask;
			});
		}

		public void CaptureInLongRun_IGraphLongOperationManager_StartAsyncOperation(IGraphLongOperationManager graphLongOperationManager, 
																					PXGraph graph)
		{
			graphLongOperationManager.StartAsyncOperation(cToken =>
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				}

				return Task.CompletedTask;
			});
		}

		public void CaptureInLongRun_IGraphLongOperationManager_StartOperation(IGraphLongOperationManager graphLongOperationManager, PXGraph graph)
		{
			graphLongOperationManager.StartOperation(cToken =>
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				}
			});
		}

		public void MemberFunc()
		{ }
	}
}