using System;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethodsAndLambdas.Sources
{
	public class MyService
	{
		public static void VoidReturningLambda()
		{
			Action lambda1 = async () => { await Task.Delay(1000); };
			Action<int> lambda2 = async x => Task.Delay(1000);
			Action<int> lambda3 = async (x) => Task.Delay(1000);
			Action<int> lambda4 = async (int x) => Task.Delay(1000));

			Action anonDelegate1 = async delegate { Task.Delay(1000); }
			Action<int> anonDelegate2 = async delegate (int x) { Task.Delay(1000); };

			Execute(async () => Task.Delay(1000));
			Execute(async delegate { Task.Delay(1000); });
		}

		private static void Execute(Action	action)
		{
			action();
		}
	}
}
