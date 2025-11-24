using PX.Data;
using PX.SM;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidViewUsageInProcessingDelegate.Sources
{
	public class UsersProcess : PXGraph<UsersProcess>
	{
		public PXCancel<Users> Cancel = null!;

		public PXProcessing<Users, Where<Users.guest, Equal<False>>> OurUsers = null!;

		public PXSetup<BlobProviderSettings> BolbSettings = null!;

		public PXSelect<Users> AllUsers = null!;

		public UsersProcess()
		{
			OurUsers.SetProcessAllCaption("Process users");
			OurUsers.SetProcessCaption("Process user");

			OurUsers.SetProcessDelegate<UsersProcess>(ProcessItem, FinallyProcess);
			OurUsers.SetProcessDelegate(
				delegate (UsersProcess graph, Users user) {
					throw new PXException();
				},
				delegate (UsersProcess graph) {
					throw new PXException();
				}
			);
			OurUsers.SetProcessDelegate(
				(UsersProcess graph, Users user) =>
				{
					throw new PXException();
				},
				(UsersProcess graph) =>
				{
					throw new PXException();
				});
			OurUsers.SetProcessDelegate(
				(UsersProcess graph, Users user) => throw new PXException(),
				(UsersProcess graph) => throw new PXException());

			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) => throw new PXException(),
											  (graph, cToken) => throw new PXException());
			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) =>
											 {
												 ProcessItem(graph, user);
												 return Task.CompletedTask;
											 },
											  (graph, cToken) =>
											  {
												  FinallyProcess(graph);
												  return Task.CompletedTask;
											  });
		}

		private static void ProcessItem(UsersProcess graph, Users item)
		{
			throw new PXException();
		}

		private static void FinallyProcess(UsersProcess graph)
		{
			throw new PXException();
		}
	}
}
