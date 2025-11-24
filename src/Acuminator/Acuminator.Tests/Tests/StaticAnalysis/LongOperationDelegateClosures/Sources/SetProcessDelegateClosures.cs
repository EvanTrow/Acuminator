using PX.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
	public class SomeGraph : PXGraph<SomeGraph>
	{
		public class SomeDAC : PXBqlTable, IBqlTable { }

		private readonly Processor processor = new Processor();

		private Processor ProcessorProperty => processor;

		private static Processor processorStatic = new Processor();


		public PXProcessing<SomeDAC> Processing;

		public SomeGraph()
		{
			object filter = null;

			Processing.SetProcessDelegate(delegate (SomeGraph graph, SomeDAC applicationProjection)
			{
				this.Clear();
			});

			Processing.SetProcessDelegate(delegate (SomeGraph graph, SomeDAC applicationProjection)
			{
				List<SomeDAC> list = new List<SomeDAC>();
				StaticFunc(list);
			});

			Processing.SetProcessDelegate(MemberFunc);
			Processing.SetProcessDelegate(StaticFunc);

			Processing.SetProcessDelegate(list => MemberFunc(list));
			Processing.SetProcessDelegate(list => StaticFunc(filter, list, false));

			//Test helper static function
			Processing.SetProcessDelegate(Processor.StaticFunc);      //No diagnostic

			//test fields and properties
			Processing.SetProcessDelegate(processor.MemberFunc);          //Should be diagnostic
			Processing.SetProcessDelegate(ProcessorProperty.MemberFunc);  //Should be diagnostic
			Processing.SetProcessDelegate(processorStatic.MemberFunc);    //No diagnostic

			Processing.SetProcessDelegate<SomeGraph>((graph, dac) => StaticFuncWithNoInput(), graph => graph.FinallyHandler());             // No diagnostic
			Processing.SetProcessDelegate<SomeGraph>((graph, dac) => MainProcessingHandler(dac), graph => FinallyHandler());                // Should be diagnostic
			Processing.SetProcessDelegate<SomeGraph>((graph, dac) => graph.MainProcessingHandler(dac), graph => FinallyHandler());          // Should be diagnostic

			Processing.SetAsyncProcessDelegate<SomeGraph>(async (graph, dac, cToken) => await StaticFuncWithNoInputAsync(cToken));          // No diagnostic
			Processing.SetAsyncProcessDelegate<SomeGraph>(async (graph, dac, cToken) => await graph.InstanceFuncWithNoInputAsync(cToken));  // No diagnostic
			Processing.SetAsyncProcessDelegate<SomeGraph>(async (graph, dac, cToken) => await InstanceFuncWithNoInputAsync(cToken));        // Should be diagnostic
			Processing.SetAsyncProcessDelegate<SomeGraph>(async (graph, dac, cToken) => await StaticFuncWithNoInputAsync(cToken), 
														  FinallyHandlerAsync);																// Should be diagnostic
		}

		public static void StaticFunc(object filter, List<SomeDAC> list, bool markOnly)
		{ }

		public static void StaticFunc(List<SomeDAC> list)
		{ }

		public void MemberFunc(List<SomeDAC> list)
		{ }

		public static void StaticFuncWithNoInput()
		{

		}

		public void MainProcessingHandler(SomeDAC dac)
		{
		}

		public void FinallyHandler()
		{
		}

		public static Task StaticFuncWithNoInputAsync(CancellationToken cancellation)
		{
			return Task.CompletedTask;
		}

		public Task InstanceFuncWithNoInputAsync(CancellationToken cancellation)
		{
			return Task.CompletedTask;
		}

		public Task FinallyHandlerAsync(PXGraph graph, CancellationToken cancellation) => Task.CompletedTask;

		private class Processor
		{
			public void MemberFunc(List<SomeDAC> list)
			{ }

			public static void StaticFunc(List<SomeDAC> list)
			{ }
		}
	}
}