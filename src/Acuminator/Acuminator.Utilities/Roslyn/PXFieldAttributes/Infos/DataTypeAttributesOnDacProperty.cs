using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Infos;

public class DataTypeAttributesOnDacProperty
{
	public ImmutableArray<DacFieldAttributeInfo> AllDataTypeAttributesOnDacProperty { get; }

	public ImmutableArray<DacFieldAttributeInfo> DataTypeAttributesWithMultipleAggregatedDataTypes { get; }

	public bool HasNonNullDataType { get; }

	public DataTypeAttributesOnDacProperty(IEnumerable<DacFieldAttributeInfo>? datatypeAttributesOnDacProperty,
										   IEnumerable<DacFieldAttributeInfo>? dataTypeAttributesWithMultipleAggregatedDataTypes,
										   bool hasNonNullDataType)
	{
		AllDataTypeAttributesOnDacProperty = datatypeAttributesOnDacProperty?.ToImmutableArray() ?? ImmutableArray<DacFieldAttributeInfo>.Empty;
		DataTypeAttributesWithMultipleAggregatedDataTypes = dataTypeAttributesWithMultipleAggregatedDataTypes?.ToImmutableArray() ?? 
															ImmutableArray<DacFieldAttributeInfo>.Empty;
		HasNonNullDataType = hasNonNullDataType;
	}
}
