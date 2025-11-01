using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SomeDac> Documents = null!;
	}

	// Abstract graph extension with PXProtectedAccess - this is still considered terminal
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess]
	public abstract class PXProtectedAccessExtension : PXGraphExtension<SomeGraph>
	{
	}

	// This graph extension inherits from PXProtectedAccess extension - should report PX1114
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedFromProtectedExtension : PXProtectedAccessExtension
	{
	}

	[PXHidden]
	public class SomeDac : PXBqlTable, IBqlTable
	{
	
	}
}