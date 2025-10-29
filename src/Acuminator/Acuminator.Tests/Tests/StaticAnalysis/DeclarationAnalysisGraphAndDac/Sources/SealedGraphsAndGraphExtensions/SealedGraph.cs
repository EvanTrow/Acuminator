using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public sealed class SealedGraph<TDac> : PXGraph<SealedGraph<TDac>>
	{
	}

	public sealed class SealedGraphWithConstraints<TDac> : PXGraph<SealedGraphWithConstraints<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public sealed class SealedGraphWithModifiers<TDac> : PXGraph<SealedGraphWithModifiers<TDac>>
	{
	}

	public sealed class SealedGraphWithMultipleTypeParameters<T1, T2> : PXGraph<SealedGraphWithMultipleTypeParameters<T1, T2>>
	{
	}

	public sealed partial class SealedPartialGraph : PXGraph<SealedPartialGraph> { }

	public sealed partial class SealedPartialGraph : PXGraph<SealedPartialGraph> { }
}