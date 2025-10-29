using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Extension inherits from abstract graph extension without PXProtectedAccess - should NOT report diagnostic
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class InheritedFromAbstractExt : AbstractExtension
	{
	}

	public abstract class AbstractExtension : PXGraphExtension<SomeGraph>
	{
	}


	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess]
	public abstract class InheritedFromAbstractExtensionWithPXProtectedAccessMembers : AbstractExtensionWithPXProtectedAccessMembers
	{
	}

	// Is treated as abstract despite having PXProtectedAccess members
	public abstract class AbstractExtensionWithPXProtectedAccessMembers : PXGraphExtension<SomeGraph>
	{
		[PXProtectedAccess]
		protected abstract void Foo();
	}

	// Extension inherits from generic non-abstract extension - should NOT report diagnostic
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedFromMultiGeneric : MultiGenericExtension<SomeDac, SomeDac>
	{
	}

	// Non-abstract generic extension with multiple type parameters
	public class MultiGenericExtension<TDac1, TDac2> : PXGraphExtension<SomeGraph>
	where TDac1 : class, IBqlTable, new()
	where TDac2 : class, IBqlTable, new()
	{
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SomeDac> Documents = null!;

		protected void Foo() { }
	}

	[PXHidden]
	public class SomeDac : IBqlTable
	{
	}
}