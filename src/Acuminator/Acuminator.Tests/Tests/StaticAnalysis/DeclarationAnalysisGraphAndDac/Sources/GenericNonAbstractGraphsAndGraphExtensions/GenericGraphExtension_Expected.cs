using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public abstract class GenericGraphExtension<TGraph> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	{
	}

	public abstract class GenericGraphExtensionWithMultipleTypeParameters<TGraph, TDac> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}
}