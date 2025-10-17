#nullable enable
using System;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC missing only CreatedByID field")]
	public class DacAddMissingCreatedByIdBetweenDacFields : IBqlTable
	{
		#region tstamp
		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string? CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual System.DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual System.Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string? LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual System.DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		#region ID
		[PXDBIdentity(IsKey = true)]
		public virtual int? ID { get; set; }
		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public virtual string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}
}