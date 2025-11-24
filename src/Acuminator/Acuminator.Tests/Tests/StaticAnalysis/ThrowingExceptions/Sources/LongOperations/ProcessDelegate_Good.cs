using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PX.Data;
using PX.SM;

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

			OurUsers.SetProcessDelegate(ProcessItem);
			OurUsers.SetProcessDelegate(delegate (Users user) {
				throw new PXException();
			});
			OurUsers.SetProcessDelegate((Users user) =>
			{
				throw new PXException();
			});
			OurUsers.SetProcessDelegate((Users user) => throw new PXException());

			OurUsers.SetProcessDelegate(ProcessItemList);
			OurUsers.SetProcessDelegate(delegate (List<Users> userList) {
				throw new PXException();
			});
			OurUsers.SetProcessDelegate((List<Users> userList) =>
			{
				throw new PXException();
			});
			OurUsers.SetProcessDelegate((List<Users> userList) => throw new PXException());

			OurUsers.SetAsyncProcessDelegate(async (Users user, CancellationToken cToken) =>
			{
				await Task.Delay(5, cToken);
				throw new PXException();
			});

			OurUsers.SetAsyncProcessDelegate((List<Users> userList, CancellationToken cToken) => throw new PXException());
		}

		private static void ProcessItem(Users user)
		{
			throw new PXException();
		}

		private static void ProcessItemList(List<Users> userList) =>
			throw new PXException();
	}
}
