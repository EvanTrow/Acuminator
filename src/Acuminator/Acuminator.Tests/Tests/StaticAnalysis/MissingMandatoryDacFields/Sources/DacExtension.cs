#nullable enable
using PX.Data;

namespace Acuminator.Tests.Sources
{
	// DAC Extensions should not be checked for mandatory fields
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <exclude/>    
	public sealed class TestDacExtension : PXCacheExtension<TestDacWithAllFields>
	{
		#region CustomField
		[PXDBString(100)]
		public string? CustomField { get; set; }
		public abstract class customField : PX.Data.BQL.BqlString.Field<customField> { }
		#endregion
	}
}