using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		private readonly int _icount;

		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			_icount = Users.Select().Count;
		}

		public void Initialize() { }

		public override void Configure(PXScreenConfiguration graph) => base.Configure(graph);
	}
}
