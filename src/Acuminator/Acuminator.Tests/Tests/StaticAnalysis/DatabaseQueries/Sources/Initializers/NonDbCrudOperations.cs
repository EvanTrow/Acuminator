using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;
using System.Collections.Generic;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UserEntryExt : PXGraphExtension<UserEntry>
	{
		public PXSelect<Users> AllUsers;

		public UserEntryExt()
		{
			Users user = AllUsers.Cache.CreateInstance() as Users;

			AllUsers.Insert(user);
			AllUsers.Update(user);
			AllUsers.Delete(user);

			AllUsers.Cache.Insert(user);
			AllUsers.Cache.Update(user);
			AllUsers.Cache.Delete(user);
		}

		public override void Initialize()
		{
			Users user = AllUsers.Cache.CreateInstance() as Users;

			AllUsers.Insert(user);
			AllUsers.Update(user);
			AllUsers.Delete(user);

			AllUsers.Cache.Insert(user);
			AllUsers.Cache.Update(user);
			AllUsers.Cache.Delete(user);
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			Users user = AllUsers.Cache.CreateInstance() as Users;

			AllUsers.Insert(user);
			AllUsers.Update(user);
			AllUsers.Delete(user);

			AllUsers.Cache.Insert(user);
			AllUsers.Cache.Update(user);
			AllUsers.Cache.Delete(user);
		}
	}

	public class UserEntry : PXGraph
	{
	}
}
