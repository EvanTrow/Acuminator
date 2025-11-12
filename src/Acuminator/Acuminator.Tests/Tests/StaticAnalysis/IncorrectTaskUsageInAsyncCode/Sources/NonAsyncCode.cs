using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public void NonAsyncMethod()
		{
			var x = GetValue();
			var str = GetString();
			DoWork();
		}

		public int GetValue()
		{
			return 42;
		}

		public string GetString()
		{
			return "test";
		}

		public void DoWork()
		{
			Console.WriteLine("work");
		}
	}
}
