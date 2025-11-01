#nullable enable
using System;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC missing multiple audit fields")]
	public class DacMissingMultipleAuditFields : PXBqlTable, IBqlTable
	{
		#region tstamp
		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string? CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual System.Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string? LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region DacID
		[PXDBIdentity(IsKey = true)]
		public virtual int? DacID { get; set; }

		public abstract class dacID : PX.Data.BQL.BqlInt.Field<dacID> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public virtual string? Description { get; set; }

		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}
}