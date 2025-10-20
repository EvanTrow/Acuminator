#nullable enable
using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC with All Mandatory Fields")]
	public class DacWithAllMandatoryFields : IBqlTable
	{
		#region DacId
		[PXDBIdentity(IsKey = true)]
		public virtual int? DacId { get; set; }
		public abstract class dacId : PX.Data.BQL.BqlInt.Field<dacId> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public virtual string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion

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
		[PXDBCreatedByScreenID]
		public virtual string? CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string? LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
	}
}