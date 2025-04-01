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

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<APInvoice.docBal> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void APInvoice_BranchID_CacheAttached(PXCache sender) { }


		protected virtual void APInvoice_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e){ }

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e){ }

		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e){ }

		protected virtual void _(Events.FieldSelecting<APInvoice, APInvoice.docBal> e){ }

		// Another generic event handler, should be recognized
		protected virtual void AnotherFieldSelectingHandler(Events.FieldSelecting<APInvoice, APInvoice.docBal> e) { }

		// Interceptor override in the same class, should be recognized
		protected virtual void _(Events.FieldSelecting<APInvoice, APInvoice.docBal> e, PXFieldSelecting fieldSelectingBase) { }


		protected virtual void _(Events.RowUpdating<APInvoice> e){}

		protected virtual void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e){}
	}




	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExtension : PXGraphExtension<SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		// Interceptor override - classic event handler
		protected virtual void APInvoice_DocBal_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated fieldUpdatedBase)
		{
			fieldUpdatedBase(cache, e);
		}

		// Incorrect generic interceptor override, delegate type should not be recognized
		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e, Events.FieldUpdated<APInvoice, APInvoice.docBal>.EventDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e);
		}

		// Correct generic interceptor override
		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e, PXFieldUpdating fieldUpdatingBase) =>
			fieldUpdatingBase(e.Cache, e.Args);

		// Additional event handler 
		protected virtual void _(Events.FieldSelecting<APInvoice, APInvoice.docBal> e) { }

		// "Simple" PXOverride of a classic event handler
		[PXOverride]
		public void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e) { }

		// "Complex" PXOverride of a classic event handler
		[PXOverride]
		public void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated rowUpdatedBase) { }

		// "Simple" PXOverride of a generic event handler
		[PXOverride]
		protected virtual void _(Events.RowUpdating<APInvoice> e) { }
	}





	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ChainedGraphExtension : PXGraphExtension<SomeGraphExtension, SomeGraph>
	{
		[PXOverride]
		public virtual void Test(object a, Action<object> baseAction) { }

		// Incorrect interceptor override attempt - cache attached event does not support interceptors
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void APInvoice_DocBal_CacheAttached(PXCache sender, Action<PXCache> baseCacheAttached) { }

		// Cache attached supports PXOverrides and should be recognized
		[PXOverride]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void APInvoice_BranchID_CacheAttached(PXCache sender, Action<PXCache> baseCacheAttached) { }

		// interceptor override should be recognized
		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.docBal> e, Events.FieldUpdated<APInvoice, APInvoice.docBal>.ClassicDelegate fieldUpdatedBase)
		{
			fieldUpdatedBase(e.Cache, e.Args);
		}

		// interceptor override should be recognized
		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.docBal> e, PXFieldUpdating fieldUpdatingBase)
		{
			fieldUpdatingBase(e.Cache, e.Args);
		}

		// "Complex" PXOverride of a generic event handler. It is no supported by the platfrom yet due to a bug, but should be recognized by Acuminator
		[PXOverride]
		protected virtual void _(Events.RowUpdating<APInvoice> e, Events.RowUpdating<APInvoice>.EventDelegate rowUpdatingBase) { }
	}
}