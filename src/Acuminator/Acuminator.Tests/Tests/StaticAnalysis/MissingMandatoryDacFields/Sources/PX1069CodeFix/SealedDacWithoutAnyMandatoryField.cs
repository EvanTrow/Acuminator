#nullable enable
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("Sealed DAC without any mandatory fields")]
	public sealed class SealedDacWithoutAnyMandatoryField : PXBqlTable, IBqlTable
	{
		#region ID
		[PXDBIdentity(IsKey = true)]
		public int? ID { get; set; }
		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}
}