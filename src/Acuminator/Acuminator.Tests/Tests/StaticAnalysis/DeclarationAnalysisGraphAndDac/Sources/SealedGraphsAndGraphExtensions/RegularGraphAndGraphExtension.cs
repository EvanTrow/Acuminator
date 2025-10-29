using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class SomeGraph : PXGraph<SomeGraph>
	{
	}

	public abstract class SomeGenericGraph<T> : PXGraph<SomeGenericGraph<T>>
	{
	}

	public partial class SomePartialGraph : PXGraph<SomePartialGraph> { }

	public partial class SomePartialGraph : PXGraph<SomePartialGraph> { }


	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	public abstract class SomeGraphExtension<TGraph> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class PartialGenericGraphExtension : PXGraphExtension<SomeGraph> { }

	public partial class PartialGenericGraphExtension : PXGraphExtension<SomeGraph> { }
}