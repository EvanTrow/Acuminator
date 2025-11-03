using System;

using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1069 MissingMandatoryDacFields This DAC should not be reported, the goal of test is to check the projection DAC
	/// <exclude/>
	[PXCacheName("Base DAC")]
	public class BaseDac : PXBqlTable, IBqlTable
	{
		#region ID
		[PXDBIdentity(IsKey = true)]
		public int? ID { get; set; }
		public abstract class iD : BqlInt.Field<iD> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public string? Description { get; set; }
		public abstract class description : BqlString.Field<description> { }
		#endregion
	}
}
