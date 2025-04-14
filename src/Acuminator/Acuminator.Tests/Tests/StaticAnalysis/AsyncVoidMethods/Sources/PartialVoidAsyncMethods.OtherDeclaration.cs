using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethods.Sources
{
	public partial class MyService
	{
		public async partial void VoidMethodAsync()
		{
			await Task.Delay(1000);
		}

		public async partial void VoidMethodWithoutAwaitWithParametersAsync(object input, int number)
		{
		}

		public async partial void VoidGenericAsyncMethod<T>(T input)
		{
			VoidMethodAsync();
			await Task.Delay(1000);
		}
	}
}
