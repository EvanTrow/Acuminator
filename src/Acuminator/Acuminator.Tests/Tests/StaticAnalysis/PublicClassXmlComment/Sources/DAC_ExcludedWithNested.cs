using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace PX.Objects
{
	/// <exclude/>
	public class ExcludedWithNested : PXBqlTable, IBqlTable  //Should not show
	{
		public class NestedDac : PXBqlTable, IBqlTable  //Should not show
		{ } 
	}

	public class PublicDac : PXBqlTable, IBqlTable  //Should show
	{
		/// <exclude/>
		public class ExcludedNested : PXBqlTable, IBqlTable
		{
			public class NestedNested : PXBqlTable, IBqlTable  //Should not show
			{ }
		}

		public class Nested : PXBqlTable, IBqlTable { } //Should  show
	}

	public class PublicGraph : PXGraph<PublicGraph>  //Should not show
	{
		/// <exclude/>
		public class ExcludedNested : PXBqlTable, IBqlTable
		{
			public class NestedNested : PXBqlTable, IBqlTable  //Should not show
			{ }
		}

		public class Nested : PXBqlTable, IBqlTable { } //Should  show
	}
}
