using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	/// <summary>
	/// Information about a DAC field - a pair consisting of a DAC field property and a DAC BQL field declared in the same type.
	/// </summary>
	public class DacFieldInfo : IWriteableBaseItem<DacFieldInfo>, IEquatable<DacFieldInfo>
	{
		public string Name { get; }

		public ITypeSymbol DacType { get; }

		public DacPropertyInfo? PropertyInfo { get; }

		public DacBqlFieldInfo? BqlFieldInfo { get; }

		protected DacFieldInfo? _baseInfo;

		public DacFieldInfo? Base => _baseInfo;

		DacFieldInfo? IWriteableBaseItem<DacFieldInfo>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo();
			}
		}

		public int DeclarationOrder => PropertyInfo?.DeclarationOrder ?? BqlFieldInfo!.DeclarationOrder;

		/// <summary>
		/// Flag indicating whether the DAC field has a DAC field property in the containing DAC or DAC extension, 
		/// and their base and chained types.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(BqlFieldInfo))]
		public bool HasFieldPropertyEffective { get; private set; }

		/// <summary>
		/// Flag indicating whether this particular DAC field has a DAC field property in the containing DAC or DAC extension
		/// without considering its base type.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(BqlFieldInfo))]
		[MemberNotNullWhen(returnValue: true, nameof(PropertyInfo))]
		public bool HasFieldPropertyDeclared { get; }

		/// <summary>
		/// Flag indicating whether the DAC field has a DAC BQL field in the containing DAC or DAC extension, 
		/// and their base and chained types.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(PropertyInfo))]
		public bool HasBqlFieldEffective { get; private set; }

		/// <summary>
		/// Flag indicating whether this particular DAC field has a DAC BQL field property in the containing DAC or DAC extension
		/// without considering its base type.
		/// </summary>
		[MemberNotNullWhen(returnValue: true, nameof(BqlFieldInfo))]
		[MemberNotNullWhen(returnValue: false, nameof(PropertyInfo))]
		public bool HasBqlFieldDeclared { get; }

		/// <value>
		/// The type of the DAC field property.
		/// </value>
		public ITypeSymbol? PropertyType { get; private set; }

		/// <value>
		/// The non nullable type of the property. For reference types and non nullable value types it is the same as <see cref="PropertyType"/>. 
		/// For nullable value types it is the underlying type extracted from nullable. It is <c>T</c> for <see cref="Nullable{T}"/>.
		/// </value>
		public ITypeSymbol? PropertyTypeUnwrappedNullable { get; private set; }

		/// <summary>
		/// The declared BQL field data type of this DAC field.
		/// </summary>
		public ITypeSymbol? BqlFieldDataTypeDeclared { get; }

		/// <summary>
		/// The effective BQL field data type of this DAC field that is obtained through the combination of <see cref="BqlFieldDataTypeDeclared"/> 
		/// from this and base info.
		/// </summary>
		public ITypeSymbol? BqlFieldDataTypeEffective {  get; private set; }

		/// <summary>
		/// The DB boundness calculated from attributes declared on this DAC property.
		/// </summary>
		public DbBoundnessType DeclaredDbBoundness { get; }

		/// <summary>
		/// The effective bound type for this DAC field obtained by the combination of <see cref="DeclaredDbBoundness"/>s of this property's override chain. 
		/// </summary>
		public DbBoundnessType EffectiveDbBoundness { get; private set; }

		public bool IsIdentity { get; private set; }

		public bool IsKey { get; private set; }

		public bool IsAutoNumbering { get; private set; }

		public bool HasAcumaticaAttributes { get; private set; }

		/// <summary>
		/// A flag indicating whether this info object represents a non-BQL property.
		/// </summary>
		/// <remarks>
		/// A non-BQL property is a C# property declared in a DAC or a DAC extension that does not have a corresponding BQL field and does not have Acumatica attributes declared on it.
		/// </remarks>
		public bool IsNonBqlProperty => HasFieldPropertyDeclared && !HasBqlFieldEffective && !HasAcumaticaAttributes;

		/// <summary>
		/// The DAC field category.
		/// </summary>
		public DacFieldCategory FieldCategory { get; }

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacBqlFieldInfo, DacFieldInfo baseInfo) :
					   this(dacPropertyInfo, dacBqlFieldInfo)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo();
		}

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacBqlFieldInfo)
		{
			if (dacPropertyInfo == null && dacBqlFieldInfo == null)
				throw new ArgumentNullException($"Both {nameof(dacPropertyInfo)} and {nameof(dacBqlFieldInfo)} parameters cannot be null.");

			PropertyInfo  = dacPropertyInfo;
			BqlFieldInfo  = dacBqlFieldInfo;
			Name 		  = PropertyInfo?.Name ?? BqlFieldInfo!.Name.ToPascalCase();
			DacType 	  = PropertyInfo?.Symbol.ContainingType ?? BqlFieldInfo!.Symbol.ContainingType;
			FieldCategory = DacFieldCategoryExtensions.GetDacFieldCategory(Name);

			if (dacBqlFieldInfo != null)
			{
				HasBqlFieldDeclared = true;
				BqlFieldDataTypeDeclared  = dacBqlFieldInfo.BqlFieldDataTypeDeclared;
				BqlFieldDataTypeEffective = dacBqlFieldInfo.BqlFieldDataTypeEffective;
			}
			else
			{
				HasBqlFieldDeclared = false;
				BqlFieldDataTypeDeclared  = null;
				BqlFieldDataTypeEffective = null;
			}

			HasBqlFieldEffective = HasBqlFieldDeclared;

			if (dacPropertyInfo != null)
			{
				HasFieldPropertyEffective = true;
				HasFieldPropertyDeclared  = true;
				IsKey 					  = dacPropertyInfo.IsKey;
				IsIdentity 				  = dacPropertyInfo.IsIdentity;
				IsAutoNumbering 		  = dacPropertyInfo.IsAutoNumbering;
				DeclaredDbBoundness 	  = dacPropertyInfo.DeclaredDbBoundness;
				EffectiveDbBoundness 	  = dacPropertyInfo.EffectiveDbBoundness;
				HasAcumaticaAttributes 	  = dacPropertyInfo.HasAcumaticaAttributesEffective;

				PropertyType 				  = dacPropertyInfo.PropertyType;
				PropertyTypeUnwrappedNullable = dacPropertyInfo.PropertyTypeUnwrappedNullable;
			}
			else
			{
				HasFieldPropertyEffective = false;
				HasFieldPropertyDeclared  = false;
				IsKey 					  = false;
				IsIdentity 				  = false;
				IsAutoNumbering 		  = false;
				DeclaredDbBoundness 	  = DbBoundnessType.NotDefined;
				EffectiveDbBoundness 	  = DbBoundnessType.NotDefined;
				HasAcumaticaAttributes 	  = false;

				PropertyType 				  = null;
				PropertyTypeUnwrappedNullable = null;
			}
		}

		public bool IsDeclaredInType(ITypeSymbol? type) =>
			 PropertyInfo?.Symbol.IsDeclaredInType(type) ?? BqlFieldInfo!.Symbol.IsDeclaredInType(type);

		void IOverridableItem<DacFieldInfo>.CombineWithBaseInfo() => CombineWithBaseInfo();

		private void CombineWithBaseInfo()
		{
			if (_baseInfo == null)
				return;

			HasAcumaticaAttributes 	  = HasAcumaticaAttributes 	  || _baseInfo.HasAcumaticaAttributes;
			HasBqlFieldEffective 	  = HasBqlFieldEffective 	  || _baseInfo.HasBqlFieldEffective;
			HasFieldPropertyEffective = HasFieldPropertyEffective || _baseInfo.HasFieldPropertyEffective;
			IsKey 					  = IsKey 					  || _baseInfo.IsKey;
			IsIdentity 				  = IsIdentity 				  || _baseInfo.IsIdentity;
			IsAutoNumbering 		  = IsAutoNumbering 		  || _baseInfo.IsAutoNumbering;
			HasAcumaticaAttributes 	  = HasAcumaticaAttributes 	  || _baseInfo.HasAcumaticaAttributes;

			PropertyType				  ??= _baseInfo.PropertyType;
			PropertyTypeUnwrappedNullable ??= _baseInfo.PropertyTypeUnwrappedNullable;

			BqlFieldDataTypeEffective ??= _baseInfo.BqlFieldDataTypeEffective;

			EffectiveDbBoundness = DeclaredDbBoundness.Combine(_baseInfo.EffectiveDbBoundness);
		}

		public override string ToString() => Name;

		public override bool Equals(object obj) => Equals(obj as DacFieldInfo);

		public bool Equals(DacFieldInfo? other) =>
			other != null &&
			SymbolEqualityComparer.Default.Equals(PropertyInfo?.Symbol, other.PropertyInfo?.Symbol) &&
			Equals(BqlFieldInfo, other.BqlFieldInfo);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(PropertyInfo?.Symbol);
				hash = hash * 23 + (BqlFieldInfo?.GetHashCode() ?? 0);
			}
			
			return hash;
		}
	}
}
