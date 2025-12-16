using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	public class PX1001ClassWithMethod : IPrefetchable
	{
		public void Prefetch()
		{
			var graph = new PX1001MethodGraph();
			PX1001MethodGraph graph2 = new();
			var baseGraph = new PXGraph();
		}
	}

	public class PX1001MethodGraph : PXGraph<PX1001MethodGraph>
	{
	}
}
