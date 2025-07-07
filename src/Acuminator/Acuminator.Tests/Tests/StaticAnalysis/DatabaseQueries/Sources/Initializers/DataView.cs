using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;


namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UserEntryExt : PXGraphExtension<UserEntry>
	{
		private int _count;

		public PXSelect<Users> AllUsers;

		public UserEntryExt()
		{
			_count = AllUsers.Select().Count;
		}

		public override void Initialize()
		{
			_count = AllUsers.Select().Count;
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			_count = AllUsers.Select().Count;
		}
	}

	public class UserEntry : PXGraph
	{
	}
}
