using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace PX.Objects.HackathonDemo.OverrideTest
{
	public class SomeGraph : PXGraph<SomeGraph>
	{
		public virtual void Test(object a) { }

		protected virtual void APInvoice_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e)
		{

		}

		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e)
		{

		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExtension : PXGraphExtension<SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		protected virtual void APInvoice_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated fieldUpdatedBase)
		{
			fieldUpdatedBase(cache, e);
		}

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e, Events.FieldUpdated<APInvoice, APInvoice.docBal>.EventDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e);
		}

		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e, PXFieldUpdating fieldUpdatingBase)
		{

		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedGraphExtension : PXGraphExtension<SomeGraphExtension, SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e, PXFieldUpdated fieldUpdatedBase)
		{

		}

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e, Events.FieldUpdated<APInvoice, APInvoice.docBal>.ClassicDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e.Cache, e.Args);
		}

		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e, PXFieldUpdating fieldUpdatingBase)
		{
			fieldUpdatingBase(e.Cache, e.Args);
		}
	}
}