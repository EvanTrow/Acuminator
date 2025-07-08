using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
	public class UserEntryExt : PXGraphExtension<UserEntry>
	{
		private int _count;

		public PXSelect<Users> AllUsers;

		public UserEntryExt()
		{
			_count = AllUsers.View.SelectMulti().Count;
		}

		public override void Initialize()
		{
			_count = AllUsers.View.SelectMulti().Count;
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			_count = AllUsers.View.SelectMulti().Count;
		}
	}

	public class UserEntry : PXGraph
	{
	}
}
