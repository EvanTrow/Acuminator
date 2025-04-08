using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Enum;

/// <summary>
/// Compatibility between the DAC property's CLR type and the type defined by data type attributes declared on the DAC property.
/// </summary>
public enum CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes : byte
{
	/// <summary>
	/// DAC property does not have data type attributes declared on it.
	/// </summary>
	NoDataTypeAttributes,

	/// <summary>
	/// CLR type of DAC property is incompatible with the type defined by data type attributes declared on the DAC property.
	/// </summary>
	IncompatibleTypes,

	/// <summary>
	/// CLR type of DAC property is compatible with the type defined by data type attributes declared on the DAC property.
	/// </summary>
	CompatibleTypes
}

public static class CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributesHelper
{
	public static CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes GetPropertyAndDataTypeAttributesTypesCompatibility(
																				this DacPropertyInfo dacProperty,
																				ImmutableArray<ITypeSymbol> dataTypesFromDataTypeAttributes)
	{
		dacProperty.ThrowOnNull();

		switch (dataTypesFromDataTypeAttributes.Length)
		{
			case 0:
				//PXDBFieldAttribute and PXEntityAttribute without data type case
				return CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.NoDataTypeAttributes;

			case 1:
				var dataTypeFromAttributes = dataTypesFromDataTypeAttributes[0];

				if (dataTypeFromAttributes.Equals(dacProperty.PropertyTypeUnwrappedNullable, SymbolEqualityComparer.Default))
					return CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.CompatibleTypes;
				else
					return CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.IncompatibleTypes;

			default:
				// Data type attributes configure more than one CLR type for a DAC field
				return CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.IncompatibleTypes;
		}
	}
}
