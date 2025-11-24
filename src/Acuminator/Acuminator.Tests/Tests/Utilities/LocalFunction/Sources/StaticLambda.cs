using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Tests.Tests.Utilities.LocalFunction.Sources
{
	public class LocalFunctionContainer
	{
		public void Static()
		{
			Action staticAction1 = static () => Console.WriteLine("Static local function in instance method.");

			Action<int> staticAction2 = static i =>
			{
				Console.WriteLine("Static local function in instance method.");
			};

			Action<int> staticAction3 = static (int i) =>
			{
				Console.WriteLine("Static local function in instance method.");
			};

			Action<int> staticAction4 = static delegate (int i)
			{
				Console.WriteLine("Static local function in instance method.");
			};
		}

		public void Instance()
		{
			Action action1 = () =>
			{
				Console.WriteLine("Static local function in instance method.");
			};

			Action<int> action2 = i =>
			{
				Console.WriteLine("Static local function in instance method.");
			};

			Action<int> action3 = (int i) =>
			{
				Console.WriteLine("Static local function in instance method.");
			};

			Action<int> action4 = delegate (int i) {
				Console.WriteLine("Static local function in instance method.");
			};
		}
	}
}
