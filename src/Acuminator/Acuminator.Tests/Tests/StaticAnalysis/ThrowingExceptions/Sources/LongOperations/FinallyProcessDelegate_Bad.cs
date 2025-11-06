using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PX.AsyncObsolete;
using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidViewUsageInProcessingDelegate.Sources
{
	[SuppressMessage("Acuminator", "PX1050:Hardcoded strings are not allowed as parameters for localization methods and PXException constructors. " +
					 "You should use string constants from the appropriate localizable messages class.", Justification = "<Pending>")]
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
					throw new PXSetupNotEnteredException<Users>(null);
				},
				delegate (UsersProcess graph) {
					throw new PXSetupNotEnteredException<Users>(null);
				}
			);
			OurUsers.SetProcessDelegate(
				(UsersProcess graph, Users user) =>
				{
					throw new PXSetupNotEnteredException<Users>(null);
				},
				(UsersProcess graph) =>
				{
					throw new PXSetupNotEnteredException<Users>(null);
				});
			OurUsers.SetProcessDelegate(
				(UsersProcess graph, Users user) => throw new PXSetupNotEnteredException<Users>(null),
				(UsersProcess graph) => throw new PXSetupNotEnteredException<Users>(null));

			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) => Task.Delay(5, cToken),
											 (UsersProcess graph, CancellationToken cToken) => throw new PXSetupNotEnteredException<Users>(null));

			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) => Task.Delay(5, cToken),
											 (UsersProcess graph, CancellationToken cToken) => FinallyProcessAsync(graph, cToken));

			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) => Task.Delay(5, cToken),
											 async (UsersProcess graph, CancellationToken cToken) => await FinallyProcessAsync(graph, cToken));

			OurUsers.SetAsyncProcessDelegate((UsersProcess graph, Users user, CancellationToken cToken) => Task.Delay(5, cToken),
											 async delegate (UsersProcess graph, CancellationToken cToken) 
											 { 
												 await FinallyProcessAsync(graph, cToken);
												 throw new PXSetupNotEnteredException<Users>(null);
											 });
		}

		private static void ProcessItem(UsersProcess graph, Users item)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		private static void FinallyProcess(UsersProcess graph)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		private static Task FinallyProcessAsync(UsersProcess graph, CancellationToken cancellation)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}
	}
}
