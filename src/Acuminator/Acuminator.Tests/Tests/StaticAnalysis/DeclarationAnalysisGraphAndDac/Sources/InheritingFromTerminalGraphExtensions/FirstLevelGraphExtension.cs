using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraphAndDac.Sources
{
	// First level graph extension - should NOT report diagnostic
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class FirstLevelGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SomeDac> Documents = null!;
	}

	[PXHidden]
	public class SomeDac : IBqlTable
	{
	}
}