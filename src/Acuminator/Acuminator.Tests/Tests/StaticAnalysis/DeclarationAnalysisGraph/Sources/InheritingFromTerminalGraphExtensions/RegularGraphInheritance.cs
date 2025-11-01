using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Graph inheriting from another non-abstract graph - should NOT report diagnostic
	public class DerivedGraph : RegularGraph
	{
		public PXSelect<SomeDac> AdditionalDocuments = null!;
	}

	public class RegularGraph : PXGraph<RegularGraph>
	{
		public PXSelect<SomeDac> Documents = null!;
	}

	// Graph inheriting from abstract graph - should NOT report diagnostic
	public class ConcreteGraph : AbstractGraph
	{
	}

	public abstract class AbstractGraph : PXGraph<AbstractGraph>
	{
		public PXSelect<SomeDac> Documents = null!;
	}

	[PXHidden]
	public class SomeDac : PXBqlTable, IBqlTable
	{
		
	}
}