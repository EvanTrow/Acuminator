using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes.Enum;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Infos;

public sealed class DataTypeAttributesOnDacProperty
{
	private readonly DacPropertyInfo _property;

	public ImmutableArray<DacFieldAttributeInfo> AllDeclaredDatatypeAttributesOnDacProperty { get; }

	public ImmutableArray<DacFieldAttributeInfo> DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes { get; }

	/// <summary>
	/// Data types configured from data type attributes declared on property including aggregated attributes.
	/// </summary>
	public ImmutableArray<ITypeSymbol> DataTypesFromDataTypeAttributes { get; }

	/// <summary>
	/// The compatibility of property type and data types from <see cref="DataTypesFromDataTypeAttributes"/> that are configured<br/>
	/// from data type attributes declared on the property.
	/// </summary>
	public CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes PropertyAndDataTypeAttributeTypesCompatibility { get; }

	private DataTypeAttributesOnDacProperty(DacPropertyInfo property, IEnumerable<ITypeSymbol>? dataTypesFromDataTypeAttributes,
											IEnumerable<DacFieldAttributeInfo>? allDeclaredDatatypeAttributesOnDacProperty,
											IEnumerable<DacFieldAttributeInfo>? declaredDataTypeAttributesWithMultipleAggregatedDataTypes)
	{
		_property = property.CheckIfNull();
		DataTypesFromDataTypeAttributes = dataTypesFromDataTypeAttributes?.ToImmutableArray() ?? ImmutableArray<ITypeSymbol>.Empty;
		AllDeclaredDatatypeAttributesOnDacProperty = allDeclaredDatatypeAttributesOnDacProperty?.ToImmutableArray() ?? 
													 ImmutableArray<DacFieldAttributeInfo>.Empty;
		DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes = declaredDataTypeAttributesWithMultipleAggregatedDataTypes?.ToImmutableArray() ?? 
																	ImmutableArray<DacFieldAttributeInfo>.Empty;

		PropertyAndDataTypeAttributeTypesCompatibility = _property.GetPropertyAndDataTypeAttributesTypesCompatibility(DataTypesFromDataTypeAttributes);
	}

	public static DataTypeAttributesOnDacProperty CollectDataTypeAttributesFromDacProperty(DacPropertyInfo property)
	{
		var declaredAttributesWithFieldTypeMetadata = property.CheckIfNull()
															  .Attributes
															  .Where(aInfo => !aInfo.AggregatedAttributeMetadata.IsDefaultOrEmpty)
															  .ToList(capacity: property.Attributes.Length);

		HashSet<ITypeSymbol>? allDataTypesFromDataTypeAttributes = null;
		List<DacFieldAttributeInfo>? allDeclaredDatatypeAttributesOnDacProperty = null;
		List<DacFieldAttributeInfo>? declaredDataTypeAttributesWithMultipleAggregatedDataTypes = null;

		foreach (var attributeDeclaredOnProperty in declaredAttributesWithFieldTypeMetadata)
		{
			var aggregatedDataTypeAttributes = attributeDeclaredOnProperty.AggregatedAttributeMetadata
																		  .Where(atrMetadata => atrMetadata.IsFieldAttribute)
																		  .ToList(capacity: 2);
			if (aggregatedDataTypeAttributes.Count == 0)
				continue;

			allDeclaredDatatypeAttributesOnDacProperty ??= new List<DacFieldAttributeInfo>(capacity: 2);
			allDeclaredDatatypeAttributesOnDacProperty.Add(attributeDeclaredOnProperty);

			// HOT PATH optimization 
			if (aggregatedDataTypeAttributes.Count == 1)
			{
				var singleDataTypeAttribute = aggregatedDataTypeAttributes[0];

				if (singleDataTypeAttribute.DataType != null)
				{
					allDataTypesFromDataTypeAttributes ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
					allDataTypesFromDataTypeAttributes.Add(singleDataTypeAttribute.DataType!);
				}

				continue;
			}

			var aggregatedDataTypes = aggregatedDataTypeAttributes.Where(atrMetadata => atrMetadata.DataType != null)
																  .Select(atrMetadata => atrMetadata.DataType!);
			int countOfNonNullDataTypesAggregatedByAttribute = 0;

			foreach (var aggregatedDataType in aggregatedDataTypes)
			{
				allDataTypesFromDataTypeAttributes ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

				if (allDataTypesFromDataTypeAttributes.Add(aggregatedDataType))
					countOfNonNullDataTypesAggregatedByAttribute++;
			}

			if (countOfNonNullDataTypesAggregatedByAttribute > 1)
			{
				declaredDataTypeAttributesWithMultipleAggregatedDataTypes ??= new List<DacFieldAttributeInfo>(capacity: 2);
				declaredDataTypeAttributesWithMultipleAggregatedDataTypes.Add(attributeDeclaredOnProperty);
			}
		}

		return new DataTypeAttributesOnDacProperty(property, allDataTypesFromDataTypeAttributes, allDeclaredDatatypeAttributesOnDacProperty, 
												   declaredDataTypeAttributesWithMultipleAggregatedDataTypes);
	}
}
