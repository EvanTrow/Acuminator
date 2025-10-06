#nullable enable

using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC missing multiple audit fields")]
	public class DacMissingMultipleAuditFields : IBqlTable
	{
		#region Tstamp
		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual System.Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		// Missing: CreatedByScreenID, CreatedDateTime, LastModifiedByScreenID, LastModifiedDateTime

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