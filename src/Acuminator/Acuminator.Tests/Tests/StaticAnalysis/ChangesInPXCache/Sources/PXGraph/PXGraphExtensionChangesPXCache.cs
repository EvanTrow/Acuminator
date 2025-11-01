using PX.Data;
using PX.Data.WorkflowAPI;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SMAccessExtDerived : SMAccessExtBase
	{
		public SMAccessExtDerived()
		{
			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				Base.Identities.Cache.Insert(Base.Identities.Current);
				Base.Identities.Cache.Update(Base.Identities.Current);
				Base.Identities.Cache.Delete(Base.Identities.Current);

				Base.Identities.Insert(Base.Identities.Current);
				Base.Identities.Update(Base.Identities.Current);
				Base.Identities.Delete(Base.Identities.Current);
			}
		}

		public override void Initialize()
		{
			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				Base.Identities.Cache.Insert(Base.Identities.Current);
				Base.Identities.Cache.Update(Base.Identities.Current);
				Base.Identities.Cache.Delete(Base.Identities.Current);

				Base.Identities.Insert(Base.Identities.Current);
				Base.Identities.Update(Base.Identities.Current);
				Base.Identities.Delete(Base.Identities.Current);
			}
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);

			int count = Base.Identities.Select().Count;

			if (count > 0)
			{
				Base.Identities.Cache.Insert(Base.Identities.Current);
				Base.Identities.Cache.Update(Base.Identities.Current);
				Base.Identities.Cache.Delete(Base.Identities.Current);

				Base.Identities.Insert(Base.Identities.Current);
				Base.Identities.Update(Base.Identities.Current);
				Base.Identities.Delete(Base.Identities.Current);
			}
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SMAccessExtBase : PXGraphExtension<CustomSMAccessPersonalMaint>
	{
		public override void Initialize()
		{
		}
	}

	public class CustomSMAccessPersonalMaint : PXGraph<CustomSMAccessPersonalMaint>
	{
		public PXSelect<Users> Identities = null!;
	}
}
