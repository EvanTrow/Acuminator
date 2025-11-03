using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraphAndDac.Sources
{
	public class SealedGraph<TDac> : PXGraph<SealedGraph<TDac>>
	{
	}

	public class SealedGraphWithConstraints<TDac> : PXGraph<SealedGraphWithConstraints<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public class SealedGraphWithModifiers<TDac> : PXGraph<SealedGraphWithModifiers<TDac>>
	{
	}

	public class SealedGraphWithMultipleTypeParameters<T1, T2> : PXGraph<SealedGraphWithMultipleTypeParameters<T1, T2>>
	{
	}

	public partial class SealedPartialGraph : PXGraph<SealedPartialGraph> { }

	public partial class SealedPartialGraph : PXGraph<SealedPartialGraph> { }
}