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
		private int _count;

		public UserEntryExt()
		{
			_count = PXSelect<Users>.Select(Base).Count;
		}

		public override void Initialize()
		{
			_count = PXSelect<Users>.Select(Base).Count;
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			_count = PXSelect<Users>.Select(Base).Count;
		}
	}


	public class UserEntry : PXGraph<UserEntry>
	{

	}
}
