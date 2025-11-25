#nullable enable
using System;
using PX.Data;
using PX.Data.BQL;

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
		#region tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public byte[]? tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public string? CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public string? LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}