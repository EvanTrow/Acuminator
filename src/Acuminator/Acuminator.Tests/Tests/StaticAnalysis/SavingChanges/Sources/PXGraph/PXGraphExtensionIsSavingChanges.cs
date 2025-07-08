using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
	public class SMAccessExt : PXGraphExtension<SMAccessPersonalMaint>
	{
		public override void Initialize()
		{
			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				Base.Identities.Delete(Base.Identities.Current);
				Base.Actions.PressSave();
			}
		}

		public SMAccessExt()
		{
			Base.Actions.PressSave();
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			Base.Actions.PressSave();
		}
	}
}
