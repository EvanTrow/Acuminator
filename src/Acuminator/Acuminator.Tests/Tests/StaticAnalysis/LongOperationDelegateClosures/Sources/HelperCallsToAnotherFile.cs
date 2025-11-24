using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		PXProcessing<SomeDAC> _processing;

		PXAction<SomeDAC> SomeAction;

		public IEnumerable someAction(PXAdapter adapter)
		{
			var helper = new NonGraph();
			helper.RunLongRunWithoutCapture(_processing);                    // No diagnostic
			helper.SetProcessingWithGraphCapture(_processing, this);         // Show diagnostic
			helper.SetProcessingWithAdapterCapture(_processing, adapter);    // Show diagnostic

			helper.CaptureInLongRun(this);                                   // Show diagnostic
			helper.CaptureInLongRunArray(this);                              // No diagnostic
			helper.CaptureInLongRunArray(this, new[] { this });              // Show diagnostic
			helper.CaptureInLongRunArray(this, [this]);                      // Should show diagnostic, no diagnosic now due to an old version of Roslyn used, ATR-923

			helper.CaptureInLongRun_LongOperationManager_StartOperation(LongOperationManager, this);			 // Show diagnostic
			helper.CaptureInLongRun_LongOperationManager_StartAsyncOperation(LongOperationManager, this);		 // Show diagnostic
			helper.CaptureInLongRun_LongOperationManager_Await(LongOperationManager, adapter);					 // Show diagnostic
			helper.CaptureInLongRun_IGraphLongOperationManager_StartAsyncOperation(LongOperationManager, this);	 // Show diagnostic
			helper.CaptureInLongRun_IGraphLongOperationManager_StartOperation(LongOperationManager, this);		 // Show diagnostic

			return adapter.Get();
		}
	}
}