using PX.Data;
using PX.Data.BQL;

using System;
using System.Collections;

namespace PX.Objects.HackathonDemo;

public class SMUserProcess : PXGraph
{
	[PXHidden]
	public class FilterDac : PXBqlTable, IBqlTable
	{
		#region SomeFlag
		/// <inheritdoc cref="SomeFlag"/>
		public abstract class someFlag : BqlBool.Field<someFlag> { }

		[PXBool]
		[PXUIField(DisplayName = "Some Flag")]
		public virtual bool? SomeFlag
		{
			get;
			set;
		}
		#endregion
	}

	public PXFilter<FilterDac> Filter;

	public PXSelect<PX.SM.Users> Users;

	public PXAction<PX.SM.Users> SyncMyUsers;

	[PXButton]
	[PXUIField]
	public virtual void syncMyUsers()
	{
		SyncUsers();
	}

	public IEnumerable users()
	{
		SyncUsers();

		return new PXSelect<PX.SM.Users>(this).Select();
	}

	public SMUserProcess()
	{
		SyncUsers();
	}

	private void SyncUsers()
	{
		PXLongOperation.StartOperation(this, () => Console.WriteLine("Synced"));
	}
}
