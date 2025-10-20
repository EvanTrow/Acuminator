#nullable enable
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("Unbound DAC - should not be checked")]
	public class UnboundDac : IBqlTable
	{
		#region DacId
		[PXInt(IsKey = true)]
		public virtual int? DacId { get; set; }
		public abstract class dacId : PX.Data.BQL.BqlInt.Field<dacId> { }
		#endregion

		#region Description
		[PXString(255)]
		public virtual string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}
}