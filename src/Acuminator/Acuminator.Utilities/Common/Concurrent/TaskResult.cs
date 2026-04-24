using System.Runtime.InteropServices;

namespace Acuminator.Utilities.Common
{
	[StructLayout(LayoutKind.Auto)]
	public readonly record struct TaskResult<TResult>(bool IsSuccess, TResult? Result);
}
