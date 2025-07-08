using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache.Sources.PXGraph
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 0)
			{
				ChangeCache();
			}
		}

		public void Initialize() => ChangeCache();

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			ChangeCache();
		}

		private void ChangeCache()
		{
			Users.Cache.Insert(Users.Current);
			Users.Cache.Update(Users.Current);
			Users.Cache.Delete(Users.Current);
		}
	}
}
