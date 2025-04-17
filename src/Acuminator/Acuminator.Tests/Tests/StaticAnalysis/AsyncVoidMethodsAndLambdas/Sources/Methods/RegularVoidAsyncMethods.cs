using System;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethodsAndLambdas.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public async void VoidMethodAsync()
		{
			await Task.Delay(1000);
		}

		public async void VoidMethodWithoutAwaitWithParametersAsync(object input, int number)
		{
		}

		public async void VoidGenericAsyncMethod<T>(T input)
		{
			VoidMethodAsync();
			await Task.Delay(1000);
		}
	}
}
