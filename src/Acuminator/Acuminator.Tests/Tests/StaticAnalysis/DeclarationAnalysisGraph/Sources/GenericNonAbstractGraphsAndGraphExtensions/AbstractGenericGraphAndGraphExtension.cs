using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public abstract class AbstractGenericGraph<TDac> : PXGraph<AbstractGenericGraph<TDac>>
	where TDac : class, IBqlTable
	{
	}

	public abstract class AbstractGenericGraphExtension<TGraph> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	{
	}

	public abstract partial class AbstractPartialGenericGraph<TDac> : PXGraph<AbstractPartialGenericGraph<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public abstract partial class AbstractPartialGenericGraph<TDac> : PXGraph<AbstractPartialGenericGraph<TDac>>
	{ }
}