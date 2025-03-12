using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace PX.Objects.HackathonDemo.OverrideTest
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedGraphExtension : PXGraphExtension<SomeGraphExtension, SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		protected virtual void _(Events.FieldUpdated<SomeDac, SomeDac.docBal> e, PXFieldUpdated fieldUpdatedBase)
		{

		}

		protected virtual void _(Events.FieldUpdated<SomeDac, SomeDac.docBal> e, Events.FieldUpdated<SomeDac, SomeDac.docBal>.ClassicDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e.Cache, e.Args);
		}

		protected virtual void _(Events.FieldUpdating<SomeDac, SomeDac.docBal> e, PXFieldUpdating fieldUpdatingBase)
		{
			fieldUpdatingBase(e.Cache, e.Args);
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExtension : PXGraphExtension<SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		protected virtual void SomeDac_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated fieldUpdatedBase)
		{
			fieldUpdatedBase(cache, e);
		}

		protected virtual void _(Events.FieldUpdated<SomeDac, SomeDac.docBal> e, Events.FieldUpdated<SomeDac, SomeDac.docBal>.EventDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e);
		}

		protected virtual void _(Events.FieldUpdating<SomeDac, SomeDac.docBal> e, PXFieldUpdating fieldUpdatingBase)
		{}
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		public virtual void Test(object a) { }

		protected virtual void SomeDac_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{ }

		protected virtual void _(Events.FieldUpdated<SomeDac, SomeDac.docBal> e)
		{ }

		protected virtual void _(Events.FieldUpdating<SomeDac, SomeDac.docBal> e)
		{ }
	}

	/// <exclude/>
	[PXHidden]
	public class SomeDac : IBqlTable
	{
		#region DocBal
		public abstract class docBal : IBqlField
		{
		}

		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DocBal { get; set; }
		#endregion
	}
}