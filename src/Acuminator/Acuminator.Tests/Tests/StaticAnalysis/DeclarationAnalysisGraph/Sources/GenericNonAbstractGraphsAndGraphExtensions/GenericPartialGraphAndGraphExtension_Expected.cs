using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public abstract partial class GenericPartialGraph<TDac> : PXGraph<GenericPartialGraph<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public abstract partial class GenericPartialGraph<TDac> : PXGraph<GenericPartialGraph<TDac>>
	{
	}

	public abstract partial class GenericPartialGraphExtension<TGraph, TDac> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public abstract partial class GenericPartialGraphExtension<TGraph, TDac> : PXGraphExtension<TGraph>
	{ }
}