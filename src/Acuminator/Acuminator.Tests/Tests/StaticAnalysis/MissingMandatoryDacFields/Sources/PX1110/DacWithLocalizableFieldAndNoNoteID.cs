#nullable enable
using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("DAC with localizable field and without the NoteID field")]
	public class DacWithLocalizableFieldAndNoNoteID : PXBqlTable, IBqlTable
	{
		#region DacId
		[PXDBIdentity(IsKey = true)]
		public virtual int? DacId { get; set; }
		public abstract class dacId : PX.Data.BQL.BqlInt.Field<dacId> { }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		
		[PXDBLocalizableString(512, IsUnicode = true)]
		public virtual string? Description { get; set; }
		#endregion

		#region tstamp
		[PXDBTimestamp]
		public virtual byte[]? tstamp { get; set; }

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}