using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [Justification]
	public class FirstLevelGenericGraphExtension<TDac> : PXGraphExtension<SomeGraph>
	where TDac : PXBqlTable, IBqlTable, new()
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [Justification]
	public abstract class FirstLevelAbstractGraphExtension : PXGraphExtension<SomeGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [Justification]
	public abstract class FirstLevelAbstractWithPXProtectedGraphExtension : PXGraphExtension<SomeGraph>
	{
		[PXProtectedAccess]
		protected abstract void Foo();
	}


	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedGraphExtension : PXGraphExtension<FirstLevelGenericGraphExtension<SOOrder>, FirstLevelAbstractGraphExtension, 
														  FirstLevelAbstractWithPXProtectedGraphExtension, SomeGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class GenericChainedGraphExtension<TDac> : PXGraphExtension<FirstLevelGenericGraphExtension<TDac>, 
																	   FirstLevelAbstractWithPXProtectedGraphExtension, SomeGraph>
	where TDac : PXBqlTable, IBqlTable, new()
	{
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		public PXSelect<SOOrder> Orders = null!;
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