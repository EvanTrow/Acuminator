using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace PX.Objects.HackathonDemo.OverrideTest
{
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