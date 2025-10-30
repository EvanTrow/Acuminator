using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisDac.Sources
{
	[PXHidden]
	public class SomeDac : IBqlTable
	{
	}

	// Acuminator disable once PX1011 InheritanceFromPXCacheExtension [Justification]
	/// <exclude/>
	public abstract class FirstLevelAbstractDacExtension : PXCacheExtension<SomeDac>
	{
		public abstract class usrDescription : BqlString.Field<usrDescription> { }

		[PXDBString(50)]
		public string? UsrDescription { get; set; }
	}

	// Acuminator disable once PX1011 InheritanceFromPXCacheExtension [Justification]
	/// <exclude/>
	public abstract class SecondLevelAbstractDacExtension : PXCacheExtension<FirstLevelAbstractDacExtension, SomeDac>
	{
		public abstract class usrName : BqlString.Field<usrName> { }

		[PXDBString(100)]
		public string? UsrName { get; set; }
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>
	public sealed class ThirdLevelDacExtension : PXCacheExtension<SecondLevelAbstractDacExtension, FirstLevelAbstractDacExtension, SomeDac>
	{
		public abstract class usrAmount : BqlDecimal.Field<usrAmount> { }

		[PXDBDecimal]
		public decimal? UsrAmount { get; set; }
	}
}