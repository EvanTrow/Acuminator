using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Infos;

public sealed class DataTypeAttributesOnDacProperty
{
	public ImmutableArray<DacFieldAttributeInfo> AllDeclaredDatatypeAttributesOnDacProperty { get; }

	public ImmutableArray<DacFieldAttributeInfo> DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes { get; }

	public bool HasNonNullDataType { get; }

	private DataTypeAttributesOnDacProperty(IEnumerable<DacFieldAttributeInfo>? allDeclaredDatatypeAttributesOnDacProperty,
											IEnumerable<DacFieldAttributeInfo>? declaredDataTypeAttributesWithMultipleAggregatedDataTypes,
											bool hasNonNullDataType)
	{
		AllDeclaredDatatypeAttributesOnDacProperty = allDeclaredDatatypeAttributesOnDacProperty?.ToImmutableArray() ?? 
													 ImmutableArray<DacFieldAttributeInfo>.Empty;
		DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes = declaredDataTypeAttributesWithMultipleAggregatedDataTypes?.ToImmutableArray() ?? 
																	ImmutableArray<DacFieldAttributeInfo>.Empty;
		HasNonNullDataType = hasNonNullDataType;
	}

	public static DataTypeAttributesOnDacProperty CollectDataTypeAttributesFromDacProperty(DacPropertyInfo property)
	{
		var declaredAttributesWithFieldTypeMetadata = property.CheckIfNull()
															  .Attributes
															  .Where(aInfo => !aInfo.AggregatedAttributeMetadata.IsDefaultOrEmpty)
															  .ToList(capacity: property.Attributes.Length);

		List<DacFieldAttributeInfo>? allDeclaredDatatypeAttributesOnDacProperty = null;
		List<DacFieldAttributeInfo>? declaredDataTypeAttributesWithMultipleAggregatedDataTypes = null;
		bool hasNonNullDataType = false;

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
				hasNonNullDataType = hasNonNullDataType || aggregatedDataTypeAttributes[0].DataType != null;
				continue;
			}

			int countOfDeclaredNonNullDataTypes = aggregatedDataTypeAttributes.Where(atrMetadata => atrMetadata.DataType != null)
																			  .Select(atrMetadata => atrMetadata.DataType!)
																			  .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
																			  .Count();
			hasNonNullDataType = hasNonNullDataType || countOfDeclaredNonNullDataTypes > 0;

			if (countOfDeclaredNonNullDataTypes > 1)
			{
				declaredDataTypeAttributesWithMultipleAggregatedDataTypes ??= new List<DacFieldAttributeInfo>(capacity: 2);
				declaredDataTypeAttributesWithMultipleAggregatedDataTypes.Add(attributeDeclaredOnProperty);
			}
		}

		return new DataTypeAttributesOnDacProperty(allDeclaredDatatypeAttributesOnDacProperty, 
												   declaredDataTypeAttributesWithMultipleAggregatedDataTypes, hasNonNullDataType);
	}
}
