using System;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethodsAndLambdas.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public static void VoidReturningLambda()
		{
			Action lambda1		= () => { Task.Delay(1000); };
			Action<int> lambda2 = x => Task.Delay(1000);
			Action<int> lambda3 = (x) => Task.Delay(1000);
			Action<int> lambda4 = (int x) => Task.Delay(1000);

			Action anonDelegate1	  = delegate { Task.Delay(1000); };
			Action<int> anonDelegate2 = delegate (int x) {Task .Delay(1000); };
		}

		public static void TaskReturningLambda()
		{
			Func<Task> lambda1			 = () => { return Task.Delay(1000); };
			Func<int, Task> lambda2		 = x => Task.Delay(1000);
			Func<int, Task<int>> lambda3 = (x) => Task.FromResult(x);
			Func<int, Task> lambda4		 = (int x) => Task.Delay(1000);

			Func<Task> anonDelegate1		   = delegate { return Task.Delay(1000); };
			Func<int, Task> anonDelegate2	   = delegate (int x) { return Task.Delay(1000); };
			Func<int, Task<int>> anonDelegate3 = delegate (int x) { return Task.FromResult(x); };
		}
	}
}
