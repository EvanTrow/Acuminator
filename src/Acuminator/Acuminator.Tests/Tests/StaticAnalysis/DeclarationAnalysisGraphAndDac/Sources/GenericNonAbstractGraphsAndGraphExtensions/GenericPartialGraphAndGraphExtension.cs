using PX.Data;

namespace PX.Objects
{
	public partial class GenericPartialGraph<TDac> : PXGraph<GenericPartialGraph<TDac>>
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public partial class GenericPartialGraph<TDac> : PXGraph<GenericPartialGraph<TDac>>
	{
	}

	public partial class GenericPartialGraphExtension<TGraph, TDac> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Documents = null!;
	}

	public partial class GenericPartialGraphExtension<TGraph, TDac> : PXGraphExtension<TGraph>
	{ }
}