using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph.Sources
{
	public class NonGenericGraph : PXGraph<NonGenericGraph>
	{
		public PXSelect<MyDac> Documents = null!;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class NonGenericGraphExtension : PXGraphExtension<NonGenericGraph>
	{
		public PXSelect<MyDac> Documents = null!;
	}


	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
		#region ID
		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD> { }

		[PXDBInt(IsKey = true)]
		public virtual int? ID { get; set; }
		#endregion
	}
}