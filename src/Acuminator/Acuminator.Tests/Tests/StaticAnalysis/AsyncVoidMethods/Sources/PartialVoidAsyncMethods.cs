using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethods.Sources
{
	public partial class MyService
	{
		public partial void VoidMethodAsync();

		public partial void VoidMethodWithoutAwaitWithParametersAsync(object input, int number);

		public partial void VoidGenericAsyncMethod<T>(T input);
	}
}
