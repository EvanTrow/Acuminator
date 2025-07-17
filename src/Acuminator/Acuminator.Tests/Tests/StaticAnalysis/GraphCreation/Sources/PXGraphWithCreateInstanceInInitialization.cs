using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.WorkflowAPI;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces.Sources
{
	public class SWKMapadocCustomerExtensionMaint : PXGraph<SWKMapadocCustomerExtensionMaint>, PX.Data.DependencyInjection.IGraphWithInitialization
	{
		public SWKMapadocCustomerExtensionMaint()
		{
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
			int key = maint.GetHashCode();
		}

		public void Initialize()
		{
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
		}

		public override void Configure(PXScreenConfiguration graph)
		{
			base.Configure(graph);
			SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();

			maint = new();
		}
	}

	public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
	{
	}
}
