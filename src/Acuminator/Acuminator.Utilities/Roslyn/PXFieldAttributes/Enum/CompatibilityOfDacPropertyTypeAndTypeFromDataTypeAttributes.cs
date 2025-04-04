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
