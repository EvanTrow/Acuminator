using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.SM;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class SMUserMaint : PXGraph<SMUserMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public PXSelect<Users> Users;

		public SMUserMaint()
		{
			int icount = Users.Select().Count;

			if (icount > 0)
			{
				Console.WriteLine("Long Operation has not been started");
			}
		}

		public void Initialize() => Console.WriteLine("Long Operation has not been started");

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			Console.WriteLine("Long Operation has not been started");
		}
	}


	public class SMUserMaintExt : PXGraphExtension<SMUserMaint>
	{
		public static bool IsActive()
		{
			Console.WriteLine("Long Operation has not been started");
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		public static bool IsActiveForGraph<TGraph>()
		{
			Console.WriteLine("Long Operation has not been started");
			return typeof(TGraph) == typeof(SMUserMaint);
		}

		public PXSelect<Users> Users = null!;

		// Acuminator disable once PX1040 ConstructorInGraphExtension [Justification]
		public SMUserMaintExt()
		{
			Console.WriteLine("Long Operation has not been started");
		}

		public override void Initialize() => Console.WriteLine("Long Operation has not been started");

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			Console.WriteLine("Long Operation has not been started");
		}
	}
}
