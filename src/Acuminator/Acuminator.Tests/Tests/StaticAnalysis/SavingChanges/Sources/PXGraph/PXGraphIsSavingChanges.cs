using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 1)
			{
				Users.Delete(Users.Current);
				Actions.PressSave();
			}
		}

		public void Initialize() =>
			Actions.PressSave();

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			Actions.PressSave();
		}
	}
}
