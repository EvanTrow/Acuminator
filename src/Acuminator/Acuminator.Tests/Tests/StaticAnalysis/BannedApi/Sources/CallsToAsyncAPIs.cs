using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo
{
	public class Service
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD105:Avoid method overloads that assume TaskScheduler.Current", Justification = "<Pending>")]
		public static async Task<bool> RunParallelAsync()
		{
			var plinq = Enumerable.Range(1, 100).AsParallel().Sum();

			Parallel.ForEach(Enumerable.Range(1, 100), i => Calculation(i));

			var hotTaskGeneric = Task.Run(() => 100*100);
			var sum = await hotTaskGeneric.ConfigureAwait(false);

			var hotTaskNonGeneric = Task.Run(() => Calculation(100));
			await hotTaskNonGeneric.ConfigureAwait(false);

			var coldTaskGeneric = new Task<int>(() => 100 * 100);
			coldTaskGeneric.Start();

			await coldTaskGeneric.ConfigureAwait(false);

			var coldTaskNonGeneric = new Task(() => Calculation(100));
			coldTaskNonGeneric.Start(TaskScheduler.Current);

			await coldTaskNonGeneric.ConfigureAwait(false);

			var hotTaskGeneric2 = Task.Factory.StartNew(() => 100 * 100);

			hotTaskNonGeneric.Wait(1000, CancellationToken.None);
			var res = hotTaskGeneric2.Result;

			res = hotTaskGeneric.GetAwaiter().GetResult();

			return true;
		}

		private static void Calculation(int i)
		{ }
	}
}
