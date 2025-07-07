using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
	public class UserEntryExt : PXGraphExtension<UserEntry>
	{
		private string _name;

		public UserEntryExt()
		{
			_name = PXSelectorAttribute.Select<Users.displayName>(Base.Caches[typeof(Users)], new Users()) as string;
		}

		public override void Initialize()
		{
			_name = PXSelectorAttribute.Select<Users.displayName>(Base.Caches[typeof(Users)], new Users()) as string;
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			_name = PXSelectorAttribute.Select<Users.displayName>(Base.Caches[typeof(Users)], new Users()) as string;
		}
	}

	public class UserEntry : PXGraph
	{
	}
}
