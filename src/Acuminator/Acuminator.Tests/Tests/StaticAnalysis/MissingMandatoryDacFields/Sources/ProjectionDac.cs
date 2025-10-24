using System;

using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXProjection(typeof(Select<BaseDac>))]
	[PXCacheName("Projection DAC")]
	public class ProjectionDac : IBqlTable
	{
		#region ID
		public abstract class iD : BqlInt.Field<iD> { }

		[PXDBIdentity(IsKey = true, BqlField = typeof(BaseDac.iD))]
		public virtual int? ID { get; set; }
		#endregion

		#region Description
		public abstract class description : BqlString.Field<description> { }

		[PXDBString(255)]
		public virtual string? Description { get; set; }
		#endregion
	}

	/// <exclude/>
	[PXProjection(typeof(Select<BaseDac>), Persistent = true)]
	[PXCacheName("Projection DAC")]
	public class PersistentProjectionDac : IBqlTable
	{
		#region ID
		public abstract class iD : BqlInt.Field<iD> { }

		[PXDBIdentity(IsKey = true, BqlField = typeof(BaseDac.iD))]
		public virtual int? ID { get; set; }
		#endregion

		#region Description
		public abstract class description : BqlString.Field<description> { }

		[PXDBString(255)]
		public virtual string? Description { get; set; }
		#endregion
	}
}
