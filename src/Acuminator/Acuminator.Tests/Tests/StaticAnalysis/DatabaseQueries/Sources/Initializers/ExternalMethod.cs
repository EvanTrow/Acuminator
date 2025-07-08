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
		private int _count;

		public UserEntryExt()
		{
			_count = GetCount();
		}

		public override void Initialize()
		{
			_count = GetCount();
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			_count = GetCount();
		}

		private int GetCount()
		{
			return PXSelect<Users>.Select(Base).Count;
		}
	}

	public class UserEntry : PXGraph
	{
	}
}
