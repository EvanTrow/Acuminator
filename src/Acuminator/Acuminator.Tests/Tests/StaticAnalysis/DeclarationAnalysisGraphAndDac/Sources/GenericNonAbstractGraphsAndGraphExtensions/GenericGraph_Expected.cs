using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public abstract class GenericGraph<TDac> : PXGraph<GenericGraph<TDac>>
	{
	}

	public abstract class GenericGraphWithConstraints<TDac> : PXGraph<GenericGraphWithConstraints<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public abstract class GenericGraphWithModifiers<TDac> : PXGraph<GenericGraphWithModifiers<TDac>>
	{
	}

	public abstract class GenericGraphWithMultipleTypeParameters<T1, T2> : PXGraph<GenericGraphWithMultipleTypeParameters<T1, T2>>
	{
	}
}