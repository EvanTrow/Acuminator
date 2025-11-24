using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Tests.Tests.Utilities.LocalFunction.Sources
{
	public class LocalFunctionContainer
	{
		public void InstanceMethodWithStaticLocalFunction()
		{
			StaticLocalFunction();
			InstanceLocalFunction();

			//------------------------------Local Function--------------------------------------------
			static void StaticLocalFunction()
			{
				Console.WriteLine("Hello from static local function!");
			}

			void InstanceLocalFunction()
			{
				Console.WriteLine("Hello from instance local function!");
			}
		}

		public static void StaticMethodWithStaticLocalFunction()
		{
			StaticLocalFunction();
			InstanceLocalFunction();

			//------------------------------Local Function--------------------------------------------
			static void StaticLocalFunction()
			{
				Console.WriteLine("Hello from static local function!");
			}

			void InstanceLocalFunction()
			{
				Console.WriteLine("Hello from instance local function!");
			}
		}
	}
}
