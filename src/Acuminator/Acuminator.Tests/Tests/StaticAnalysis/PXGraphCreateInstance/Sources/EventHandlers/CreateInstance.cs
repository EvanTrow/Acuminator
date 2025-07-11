using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BinExtension : PXGraphExtension<PX.Objects.IN.INSiteMaint>
	{
		protected virtual void INLocation_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus != PXTranStatus.Completed) return;

			var orderMaint = PXGraph.CreateInstance<SOOrderEntry>();
		}
	}
}
