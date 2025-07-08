using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SWKMapadocCustomerExtensionMaint : PXGraphExtension<SWKMapadocConnMaint>
	{
		public SWKMapadocCustomerExtensionMaint()
		{
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
		}

		public override void Initialize()
		{
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
			int key = maint.GetHashCode();
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			base.Configure(configuration);
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
		}
	}

	public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
	{
	}
}
