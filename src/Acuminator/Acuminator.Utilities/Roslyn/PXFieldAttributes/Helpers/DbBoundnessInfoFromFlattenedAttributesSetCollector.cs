using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes;

internal static class DbBoundnessInfoFromFlattenedAttributesSetCollector
{
	private const int MaxRecursion = 100;

	public static DbBoundnessType GetCombinedDbBoundness(IReadOnlyCollection<AttributeWithApplicationAndAggregator> flattenedAttributesWithApplications)
	{
		if (flattenedAttributesWithApplications?.Count is null or 0)
			return DbBoundnessType.NotDefined;

		// Attribute with application DTOs should be unique by (attribute type + application) pair so we can use them as a key
		// In the calculated DB boundness cache
		Dictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> calcedBoundnessByAttributeApplication = new();
		var combinedBoundness = DbBoundnessType.NotDefined;

		foreach (var attributeApplication in flattenedAttributesWithApplications)
		{
			var attributeDbBoundness = CalculateDbBoundnessForAttribute(calcedBoundnessByAttributeApplication, attributeApplication, recursionDepth: 0);
			combinedBoundness = combinedBoundness.Combine(attributeDbBoundness);

			if (combinedBoundness == DbBoundnessType.Error)
				return DbBoundnessType.Error;
		}

		return combinedBoundness;
	}

	private static DbBoundnessType CalculateDbBoundnessForAttribute(
										Dictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> calcedBoundnessByAttributeApplication, 
										AttributeWithApplicationAndAggregator attributeApplication, int recursionDepth)
	{
		if (calcedBoundnessByAttributeApplication.TryGetValue(attributeApplication, out var attributeDbBoundness))
			return attributeDbBoundness;

		attributeDbBoundness = 
			ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication.Application);

		if (!attributeApplication.HasAggregator)    // Non-aggregated attributes, can be aggregators themselves
		{
			calcedBoundnessByAttributeApplication[attributeApplication] = attributeDbBoundness;
			return attributeDbBoundness;
		}

		// If aggregated attribute does not define any DB boundness then there is no need to consider how it combines with aggregator's DB boundness.
		// It is not a data type attribute.
		// 
		// On the other hand, if the attribute has error DB boundness, then we don't need to combine it with aggregator's DB boundness
		// In fact, this means that the DB boundness of the entire original attribute placed on DAC field is error.
		// Note, that if attribute's DB boundness is unknown, we still need to consider aggregator's DB boundness because it can be error boundness. 
		if (attributeDbBoundness is DbBoundnessType.NotDefined or DbBoundnessType.Error)
		{
			calcedBoundnessByAttributeApplication[attributeApplication] = attributeDbBoundness;
			return attributeDbBoundness;
		}

		// Aggregated attributes, can be aggregators themselves
		var aggregatorDbBoundness = recursionDepth > MaxRecursion
			? DbBoundnessType.NotDefined
			: CalculateDbBoundnessForAttribute(calcedBoundnessByAttributeApplication, attributeApplication.Aggregator, recursionDepth + 1);

		var combinedBoundness = DbBoundnessTypeCombiner.CombineBoundnessFromAggregatorAndAggregatedAttributes(attributeDbBoundness, 
																											  aggregatorDbBoundness);
		calcedBoundnessByAttributeApplication[attributeApplication] = combinedBoundness;
		return combinedBoundness;
	}
}
