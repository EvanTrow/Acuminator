#nullable enable
using System;

using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC with fields added to beginning")]
	public class DacWithFieldsAddedToBeginning : IBqlTable
	{
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedDateTime
		/// <inheritdoc cref="CreatedDateTime"/>
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		/// <summary>
		/// Created date time.
		/// </summary>
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string? CreatedByScreenID { get; set; }
		#endregion
		#region LastModifiedByID
		/// <inheritdoc cref="LastModifiedByID"/>
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

		/// <summary>
		/// ID of the user who modified record last.
		/// </summary>
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
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

		#region Tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }
		#endregion
	}
}