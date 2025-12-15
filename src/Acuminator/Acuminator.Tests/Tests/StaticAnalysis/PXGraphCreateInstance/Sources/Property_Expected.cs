using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	class PX1001ClassWithProperty
	{
		public PXGraph Graph
		{
			get { return PXGraph.CreateInstance<PX1001PropertyGraph>(); }
		}

		public PXGraph Graph2
		{
			get { return PXGraph.CreateInstance<PXGraph>(); }
		}
	}

	public class PX1001PropertyGraph : PXGraph<PX1001PropertyGraph>
	{
	}
}
