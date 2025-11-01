using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [Justification]
	public class FirstLevelNonAbstractGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [Justification]
	[PXProtectedAccess]
	public abstract class FirstLevelWithPXProtectedGraphExtension : PXGraphExtension<SomeGraph>
	{
		[PXProtectedAccess]
		protected abstract void Foo();
	}


	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedGraphExtension : PXGraphExtension<FirstLevelNonAbstractGraphExtension, FirstLevelWithPXProtectedGraphExtension, SomeGraph>
	{
	}


	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SOOrder> Orders = null!;

		protected void Foo() { }
	}

	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class SOOrder : PXBqlTable, IBqlTable
	{
		public abstract class orderNbr : BqlString.Field<orderNbr> { }

		[PXDBString(15, IsKey = true)]
		public virtual string? OrderNbr { get; set; }
	}
}