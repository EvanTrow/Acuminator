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
		}

		public void Initialize()
		{
			int icount = Users.Select().Count;
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			int icount = Users.Select().Count;
		}
	}
}
