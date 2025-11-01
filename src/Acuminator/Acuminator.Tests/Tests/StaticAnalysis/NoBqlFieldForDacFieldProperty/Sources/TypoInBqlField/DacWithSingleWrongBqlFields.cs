using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithSingleWrongBqlFields : PXBqlTable, IBqlTable
	{
		public abstract class n0teID : PX.Data.BQL.BqlGuid.Field<n0teID> { }

		[PXGuid]
		public Guid? NoteID { get; set; }

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasNoteID => NoteID != null;
	}
}