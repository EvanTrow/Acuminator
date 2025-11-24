using PX.Data;
using PX.SM;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

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

			OurUsers.SetProcessDelegate(ProcessItem);
			OurUsers.SetProcessDelegate(delegate (Users user) {
				throw new PXSetupNotEnteredException<Users>(null);
			});
			OurUsers.SetProcessDelegate((Users user) =>
			{
				throw new PXSetupNotEnteredException<Users>(null);
			});
			OurUsers.SetProcessDelegate((Users user) => throw new PXSetupNotEnteredException<Users>(null));

			OurUsers.SetProcessDelegate(ProcessItemList);
			OurUsers.SetProcessDelegate(delegate (List<Users> userList) {
				throw new PXSetupNotEnteredException<Users>(null);
			});
			OurUsers.SetProcessDelegate((List<Users> userList) =>
			{
				throw new PXSetupNotEnteredException<Users>(null);
			});
			OurUsers.SetProcessDelegate((List<Users> userList) => throw new PXSetupNotEnteredException<Users>(null));

			OurUsers.SetAsyncProcessDelegate(async (Users user, CancellationToken cToken) =>
			{
				await Task.Delay(5, cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});

			OurUsers.SetAsyncProcessDelegate(async (Users user, CancellationToken cToken) =>
			{
				await Task.Delay(5, cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});

			OurUsers.SetAsyncProcessDelegate((Users user, CancellationToken cToken) => ProcessItemAsync(user));
			OurUsers.SetAsyncProcessDelegate((List<Users> userList, CancellationToken cToken) => throw new PXSetupNotEnteredException<Users>(null));
			OurUsers.SetAsyncProcessDelegate((List<Users> userList, CancellationToken cToken) => ProcessItemListAsync(userList));

			OurUsers.SetAsyncProcessDelegate(async delegate (List<Users> userList, CancellationToken cToken)
			{
				await Task.Delay(5, cToken);
				throw new PXSetupNotEnteredException<Users>(null);
			});
		}

		private static void ProcessItem(Users user)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		private static Task ProcessItemAsync(Users user)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		private static void ProcessItemList(List<Users> userList)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}

		private static Task ProcessItemListAsync(List<Users> userList)
		{
			throw new PXSetupNotEnteredException<Users>(null);
		}
	}
}
