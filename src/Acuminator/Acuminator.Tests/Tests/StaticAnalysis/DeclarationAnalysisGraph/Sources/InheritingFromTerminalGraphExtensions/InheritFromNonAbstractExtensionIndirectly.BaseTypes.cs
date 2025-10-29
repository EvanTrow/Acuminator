using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Non-abstract graph extension - this is a terminal extension
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class NonAbstractGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class DerivedAbstractGraphExtension : NonAbstractGraphExtension, ISomeInterface
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

	public interface ISomeInterface
	{ }
}