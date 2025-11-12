using System;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode.Sources
{
	public record MyRecord(Task<int> Task);

	public readonly record struct MyRecordStruct(int Number, ValueTask<int> Task, int LineNumber);

	public class MyService(Task task)
	{
		public int this[Task task] => 0;

		public MyService(Task task, int i) : this (task)
		{
		}

		public object Process1(object data, Task task)
		{
			return data;
		}

		public object Process2(object data, Task<string> task1, ValueTask task2, ValueTask<int> task3) =>
			data;
	}
}
