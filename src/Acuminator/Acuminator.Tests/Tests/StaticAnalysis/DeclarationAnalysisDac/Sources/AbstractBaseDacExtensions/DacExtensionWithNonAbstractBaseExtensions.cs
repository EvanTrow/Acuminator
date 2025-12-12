using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisDac.Sources
{
	[PXHidden]
	public class SomeDac : PXBqlTable, IBqlTable
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>
	public sealed class FirstLevelDacExtension : PXCacheExtension<SomeDac>
	{
		public abstract class usrCustomField : BqlString.Field<usrCustomField> { }

		[PXDBString(50)]
		public string? UsrCustomField { get; set; }
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>
	public sealed class SecondLevelDacExtension : PXCacheExtension<FirstLevelDacExtension, SomeDac>
	{
		public abstract class usrAnotherField : BqlString.Field<usrAnotherField> { }

		[PXDBString(100)]
		public string? UsrAnotherField { get; set; }
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>
	public sealed class ThirdLevelDacExtension : PXCacheExtension<SecondLevelDacExtension, FirstLevelDacExtension, SomeDac>
	{
		public abstract class usrCustomAmount : BqlDecimal.Field<usrCustomAmount> { }

		[PXDBDecimal]
		public decimal? UsrCustomAmount { get; set; }
	}
}