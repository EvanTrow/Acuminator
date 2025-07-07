using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
	public class UserEntry : PXGraph, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		private int _count;

		public UserEntry()
		{
			_count = PXSelect<Users>.Select(this).Count;
		}

		public void Initialize()
		{
			_count = PXSelect<Users>.Select(this).Count;
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			_count = PXSelect<Users>.Select(this).Count;
		}
	}
}
