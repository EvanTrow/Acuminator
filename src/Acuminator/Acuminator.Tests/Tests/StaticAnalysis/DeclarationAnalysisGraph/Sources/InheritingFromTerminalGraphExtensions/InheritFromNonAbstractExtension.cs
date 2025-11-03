using PX.Data;
using PX.Data.DependencyInjection;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SomeDac> Documents = null!;
	}

	// Non-abstract graph extension - this is a terminal extension
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class NonAbstractGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	// This graph extension inherits from a non-abstract extension - should report PX1114
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedGraphExtension : NonAbstractGraphExtension, ISomeInterface
	{
	}

	public interface ISomeInterface 
	{ }

	[PXHidden]
	public class SomeDac : PXBqlTable, IBqlTable
	{
		
	}
}