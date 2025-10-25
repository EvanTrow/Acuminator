using PX.Data;

namespace PX.Objects
{
	using System;
	using PX.Data;

	// Generic graph extension - should report diagnostic
	public class GenericGraphExtension<T> : PXGraphExtension<SomeGraph>
	{
		public PXSelect<MyDac> Documents;
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
	}

	public class MyDac : IBqlTable
	{
		[PXDBInt(IsKey = true)]
		public virtual int? ID { get; set; }
		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD> { }
	}
}