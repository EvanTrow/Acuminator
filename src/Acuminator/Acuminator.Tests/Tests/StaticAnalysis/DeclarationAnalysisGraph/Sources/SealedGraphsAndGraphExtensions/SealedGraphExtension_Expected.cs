using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SealedGraphExtension : PXGraphExtension<MyGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class SealedPartialGraphExtension : PXGraphExtension<MyGraph> { }

	public partial class SealedPartialGraphExtension : PXGraphExtension<MyGraph> { }

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}