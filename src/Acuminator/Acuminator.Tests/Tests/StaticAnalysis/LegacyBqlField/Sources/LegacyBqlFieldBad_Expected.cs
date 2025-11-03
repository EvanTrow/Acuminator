#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using PX.Data;
using PX.Objects.CR;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	[SuppressMessage("Acuminator", "PX1069:DAC must declare mandatory audit and timestamp DAC fields", Justification = "<Pending>")]
	public class BadDac : PXBqlTable, IBqlTable
	{
		public abstract class legacyBoolField : PX.Data.BQL.BqlBool.Field<legacyBoolField> { }
		[PXBool]
		public bool? LegacyBoolField { get; set; }

		public abstract class legacyByteField : PX.Data.BQL.BqlByte.Field<legacyByteField> { }
		[PXByte]
		public Byte? LegacyByteField { get; set; }

		public abstract class legacyShortField : PX.Data.BQL.BqlShort.Field<legacyShortField> { }
		[PXShort]
		public short? LegacyShortField { get; set; }

		public abstract class legacyIntField : PX.Data.BQL.BqlInt.Field<legacyIntField> { }
		[PXInt]
		public Int32? LegacyIntField { get; set; }

		public abstract class legacyLongField : PX.Data.BQL.BqlLong.Field<legacyLongField> { }
		[PXLong]
		public long? LegacyLongField { get; set; }

		public abstract class legacyFloatField : PX.Data.BQL.BqlFloat.Field<legacyFloatField> { }
		[PXFloat]
		public Single? LegacyFloatField { get; set; }

		public abstract class legacyDoubleField : PX.Data.BQL.BqlDouble.Field<legacyDoubleField> { }
		[PXDouble]
		public double? LegacyDoubleField { get; set; }

		public abstract class legacyDecimalField : PX.Data.BQL.BqlDecimal.Field<legacyDecimalField> { }
		[PXDecimal]
		public Decimal? LegacyDecimalField { get; set; }

		public abstract class legacyStringField : PX.Data.BQL.BqlString.Field<legacyStringField> { }
		[PXString]
		public string LegacyStringField { get; set; }

		public abstract class legacyDateField : PX.Data.BQL.BqlDateTime.Field<legacyDateField> { }
		[PXDate]
		public DateTime? LegacyDateField { get; set; }

		public abstract class legacyGuidField : PX.Data.BQL.BqlGuid.Field<legacyGuidField> { }
		[PXGuid]
		public Guid? LegacyGuidField { get; set; }

		public abstract class legacyBinaryField : PX.Data.BQL.BqlByteArray.Field<legacyBinaryField> { }
		[PXDBBinary]
		public byte[] LegacyBinaryField { get; set; }

		public abstract class attributes : PX.Objects.CR.BqlAttributes.Field<attributes> { }
		[CRAttributesField(typeof(CRQuote.opportunityClassID))]
		public virtual string[] Attributes { get; set; }

		#nullable enable

		#region LegacyNullableStringField
		public abstract class legacyNullableStringField : PX.Data.BQL.BqlString.Field<legacyStringField> { }

		[PXDBString(50)]
		public string? LegacyNullableStringField { get; set; }
		#endregion

		#region LegacyNullableBinaryField
		public abstract class legacyNullableBinaryField : PX.Data.BQL.BqlByteArray.Field<legacyBinaryField> { }

		[PXDBBinary]
		public byte[]? LegacyNullableBinaryField { get; set; }
		#endregion
	}
}