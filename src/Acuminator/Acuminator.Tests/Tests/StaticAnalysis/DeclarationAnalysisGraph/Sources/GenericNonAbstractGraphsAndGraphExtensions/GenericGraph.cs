using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class GenericGraph<TDac> : PXGraph<GenericGraph<TDac>>
	{
	}

	public class GenericGraphWithConstraints<TDac> : PXGraph<GenericGraphWithConstraints<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public sealed class GenericGraphWithModifiers<TDac> : PXGraph<GenericGraphWithModifiers<TDac>>
	{
	}

	public class GenericGraphWithMultipleTypeParameters<T1, T2> : PXGraph<GenericGraphWithMultipleTypeParameters<T1, T2>>
	{
	}
}