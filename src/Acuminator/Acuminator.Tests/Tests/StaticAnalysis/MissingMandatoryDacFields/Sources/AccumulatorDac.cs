#nullable enable
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXAccumulator]
	[PXCacheName("Accumulator DAC - should not be checked")]
	public class AccumulatorDac : IBqlTable
	{
		#region DacId
		[PXDBIdentity(IsKey = true)]
		public virtual int? DacId { get; set; }
		public abstract class dacId : PX.Data.BQL.BqlInt.Field<dacId> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public virtual string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}

	/// <exclude/>
	[DerivedAccumulator]
	[PXCacheName("Derived Accumulator DAC - should not be checked")]
	public class DerivedAccumulatorDac : IBqlTable
	{
		#region ID
		[PXDBIdentity(IsKey = true)]
		public virtual int? ID { get; set; }
		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD> { }
		#endregion

		#region Description
		[PXDBString(255)]
		public virtual string? Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}

	public class DerivedAccumulator : PXAccumulatorAttribute
	{
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns) => 
			base.PrepareInsert(sender, row, columns);
	}
}