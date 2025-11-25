using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the Acumatica data type attribute.
	/// </summary>
	public class DataTypeAttributeInfo : IEquatable<DataTypeAttributeInfo>
	{
		/// <summary>
		/// The type of the attribute.
		/// </summary>
		public INamedTypeSymbol AttributeType { get; }

		public ITypeSymbol? DataType { get; }

		public FieldTypeAttributeKind Kind { get; }

		public bool IsCalculatedOnDbSide =>
			Kind == FieldTypeAttributeKind.PXDBCalcedAttribute || Kind == FieldTypeAttributeKind.PXDBScalarAttribute;

		public bool IsFieldAttribute =>
			Kind == FieldTypeAttributeKind.BoundTypeAttribute || 
			Kind == FieldTypeAttributeKind.UnboundTypeAttribute || 
			Kind == FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute;

		public DataTypeAttributeInfo(FieldTypeAttributeKind attributeKind, INamedTypeSymbol attributeType, ITypeSymbol? fieldType)
		{
			AttributeType = attributeType.CheckIfNull();
			DataType 	  = fieldType;
			Kind 		  = attributeKind;
		}

		public virtual DbBoundnessType GetDbBoundness() => Kind switch
		{
			FieldTypeAttributeKind.BoundTypeAttribute 			 => DbBoundnessType.DbBound,
			FieldTypeAttributeKind.UnboundTypeAttribute 		 => DbBoundnessType.Unbound,
			FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute => DbBoundnessType.Unknown,
			FieldTypeAttributeKind.PXDBScalarAttribute 			 => DbBoundnessType.PXDBScalar,
			FieldTypeAttributeKind.PXDBCalcedAttribute 			 => DbBoundnessType.PXDBCalced,
			_ 													 => DbBoundnessType.NotDefined
		};
		
		public override bool Equals(object obj) => Equals(obj as DataTypeAttributeInfo);

		public virtual bool Equals(DataTypeAttributeInfo? other)
		{
			if (ReferenceEquals(this, other))
				return true;
			else if (other == null)
				return false;

			return Kind == other.Kind && GetType() == other.GetType() &&
				   SymbolEqualityComparer.Default.Equals(DataType, other.DataType) &&
				   SymbolEqualityComparer.Default.Equals(AttributeType, other.AttributeType);
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Kind.GetHashCode();
				hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(DataType);
				hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(AttributeType);
			}

			return hash;
		}

		public override string ToString() => $"{Kind.ToString()} {AttributeType}";
	}
}
