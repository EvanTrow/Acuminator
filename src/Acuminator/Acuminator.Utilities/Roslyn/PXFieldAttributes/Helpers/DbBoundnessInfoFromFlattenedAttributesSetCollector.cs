using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about a set of Acumatica atttributes and their DB boundness.
	/// </summary>
	internal readonly record struct DbBoundnessInfoForAttributesSet(
										IReadOnlyDictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> AttributesBoundness,
										DbBoundnessType CombinedBoundness)
	{  }


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

		public static DbBoundnessInfoForAttributesSet CollectInfoFromAttributes(
														IReadOnlyCollection<AttributeWithApplicationAndAggregator> flattenedAttributesWithApplications)
		{
			if (flattenedAttributesWithApplications?.Count is null or 0)
			{
				return new(ImmutableDictionary<AttributeWithApplicationAndAggregator, DbBoundnessType>.Empty, DbBoundnessType.NotDefined);
			}

			// Attribute with application DTOs should be unique by (attribute type + application) pair so we can use them as a key
			// In the calculated DB boundness cache
			Dictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> calcedBoundnessByAttributeApplication = new();
			var combinedBoundness = DbBoundnessType.NotDefined;

			foreach (var attributeApplication in flattenedAttributesWithApplications)
			{
				var attributeDbBoundness = CalculateDbBoundnessForAttribute(calcedBoundnessByAttributeApplication, attributeApplication, recursionDepth: 0);
				combinedBoundness = combinedBoundness.Combine(attributeDbBoundness);
			}

			return new(calcedBoundnessByAttributeApplication, combinedBoundness);
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
			// On the other hand, if the attribute has error or unknown DB boundness, then we can't combine it with aggregator's DB boundness
			// In fact, this means that the DB boundness of the entire aggregator attribute is unknown or error
			if (attributeDbBoundness is DbBoundnessType.NotDefined or DbBoundnessType.Error or DbBoundnessType.Unknown)
			{
				calcedBoundnessByAttributeApplication[attributeApplication] = attributeDbBoundness;
				return attributeDbBoundness;
			}

			// Aggregated attributes, can be aggregators themselves
			var aggregatorDbBoundness = recursionDepth > MaxRecursion
				? DbBoundnessType.NotDefined
				: CalculateDbBoundnessForAttribute(calcedBoundnessByAttributeApplication, attributeApplication.Aggregator, recursionDepth + 1);

			var combinedBoundness = CombineBoundnessFromAggregatorAndAggregatedAttributes(attributeDbBoundness, aggregatorDbBoundness);
			calcedBoundnessByAttributeApplication[attributeApplication] = combinedBoundness;
			return combinedBoundness;
		}

		private static DbBoundnessType CombineBoundnessFromAggregatorAndAggregatedAttributes(DbBoundnessType aggregatedDbBoundness,
																							 DbBoundnessType aggregatorDbBoundness)
		{
			switch (aggregatorDbBoundness)
			{
				// If aggregator attribute is unbound then it overrides the DB boundness of the aggregated attribute only if the aggregated attribute is DB bound.
				// In that case the resulting DB boundness is unbound because the aggregator attribute won't subscribe the aggregated DB bound attribute
				// To the DB related Acumatica events. This effectively makes the aggregated attribute unbound too.
				case DbBoundnessType.Unbound:
					return aggregatedDbBoundness == DbBoundnessType.DbBound
						? DbBoundnessType.Unbound
						: aggregatedDbBoundness;
					
				// If aggregator's DB boundness is unknown or error, then the DB boundness of aggregated attributes
				// is effectively unknown or error, since we don't know how aggregator uses them
				case DbBoundnessType.Unknown:
				case DbBoundnessType.Error:
					return aggregatorDbBoundness;

				// For DB bound aggregator the DB boundness of the aggregated attribute is not overridden by the aggregator attribute
				case DbBoundnessType.DbBound:

				// For DBScalar and DBCalced the aggregator won't override the DB boundness of the aggregated attribute
				// Even if the combination of the DB boundnesses of the aggregator and aggregated attributes will produce an error later
				case DbBoundnessType.PXDBCalced:
				case DbBoundnessType.PXDBScalar:

				// Aggregator does not affect DB boundness of the aggregated attribute
				case DbBoundnessType.NotDefined:
				default:
					return aggregatedDbBoundness;
			}
		}
	}
}
