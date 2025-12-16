using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	class PX1001ClassWithField
	{
		private readonly PXGraph _field = new PX1001FieldGraph();
		private readonly PXGraph _field2 = new PXGraph();
	}

	class PX1001FieldGraph : PXGraph<PX1001FieldGraph>
	{
	}
}
