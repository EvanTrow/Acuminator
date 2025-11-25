using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes;

internal static class DbBoundnessInfoFromFlattenedAttributesSetAndMetadataCollector
{
	private const int MaxRecursion = 100;

	public static DbBoundnessType GetCombinedDbBoundness(ImmutableHashSet<AttributeWithApplicationAndAggregator> flattenedAttributesWithApplications,
														 IReadOnlyCollection<DataTypeAttributeInfo> attributesMetadata)
	{
		if (flattenedAttributesWithApplications?.Count is null or 0)
			return DbBoundnessType.NotDefined;

		// Fallback to DB boundness set explicitly by attribute applications if there is no metadata about data type attributes
		if (attributesMetadata?.Count is null or 0)
			return GetDbBoundnessSetExplicitlyByAttributeApplications(flattenedAttributesWithApplications);

		Dictionary<INamedTypeSymbol, DbBoundnessType> attributeDBBoundnessFromMetadataByAttributeType = 
			attributesMetadata.GroupBy(metadata => metadata.AttributeType, SymbolEqualityComparer.Default as IEqualityComparer<INamedTypeSymbol>)
							  .ToDictionary(group => group.Key,
											group => group.Select(metadata => metadata.GetDbBoundness()).Combine(),
											SymbolEqualityComparer.Default as IEqualityComparer<INamedTypeSymbol>);

		if (attributeDBBoundnessFromMetadataByAttributeType.Values.Any(metadataBoundness => metadataBoundness == DbBoundnessType.Error))
			return DbBoundnessType.Error;

		// Attribute with application DTOs should be unique by (attribute type + application) pair so we can use them as a key
		// In the calculated DB boundness cache
		Dictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> calcedBoundnessByAttributeApplication = new();
		DbBoundnessType combinedBoundness = DbBoundnessType.NotDefined;

		foreach (var attributeInfo in flattenedAttributesWithApplications)
		{
			var attributeDbBoundness = CalculateDbBoundnessForAttributeFromMetadataAndApplication(attributeInfo, calcedBoundnessByAttributeApplication,
																								  attributeDBBoundnessFromMetadataByAttributeType,
																								  recursionDepth: 0);
			combinedBoundness = combinedBoundness.Combine(attributeDbBoundness);

			if (combinedBoundness == DbBoundnessType.Error)
				return DbBoundnessType.Error;
		}

		return combinedBoundness;
	}

	private static DbBoundnessType GetDbBoundnessSetExplicitlyByAttributeApplications(
														ImmutableHashSet<AttributeWithApplicationAndAggregator> flattenedAttributesWithApplications)
	{
		if (flattenedAttributesWithApplications.All(attrApplication => !attrApplication.HasAggregator))
		{
			return flattenedAttributesWithApplications
						.Select(attrWithApplication => ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(
																														attrWithApplication.Application))
						.Combine();
		}

		var combinedDbBoundness = DbBoundnessInfoFromFlattenedAttributesSetCollector.GetCombinedDbBoundness(flattenedAttributesWithApplications);
		return combinedDbBoundness;
	}

	private static DbBoundnessType CalculateDbBoundnessForAttributeFromMetadataAndApplication(AttributeWithApplicationAndAggregator attributeApplication,
											Dictionary<AttributeWithApplicationAndAggregator, DbBoundnessType> calcedBoundnessByAttributeApplication,
											Dictionary<INamedTypeSymbol, DbBoundnessType> attributeDBBoundnessFromMetadataByAttributeType,
											int recursionDepth)
	{
		if (calcedBoundnessByAttributeApplication.TryGetValue(attributeApplication, out DbBoundnessType cachedAttributeDbBoundness))
			return cachedAttributeDbBoundness;

		// Even if the DB boundness is set explicitly at the application we have to calculate DB boundness from metadata
		// Because it can be inconsistent in which case explicitly set DB boundness can't apply
		DbBoundnessType dbBoundnessFromMetadata =
			attributeApplication.Type is INamedTypeSymbol attributeType && 
			attributeDBBoundnessFromMetadataByAttributeType.TryGetValue(attributeType, out DbBoundnessType calculatedDbBoundnessFromMetadata)
				? calculatedDbBoundnessFromMetadata
				: DbBoundnessType.NotDefined;

		if (dbBoundnessFromMetadata == DbBoundnessType.Error)       // Stop calculation immediately if metadata has error DB boundness
		{
			calcedBoundnessByAttributeApplication[attributeApplication] = dbBoundnessFromMetadata;
			return dbBoundnessFromMetadata;
		}

		// Get explicitly set DB boundness by this attribute application
		var explicitDbBoundnessFromApplication =
			ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication.Application);
		var combinedExplicitAndMetadataBoundness = CombineExplicitlySetAndMetadataBoundness(explicitDbBoundnessFromApplication, 
																							dbBoundnessFromMetadata);

		// If aggregated attribute does not define any DB boundness both explicitly and in metadata, 
		// then there is no need to consider how it combines with aggregator's DB boundness. It is not a data type attribute.
		// 
		// On the other hand, if the attribute has error DB boundness, then we don't need to combine it with aggregator's DB boundness
		// In fact, this means that the DB boundness of the entire original attribute placed on DAC field is error.
		// Note, that if attribute's DB boundness is unknown, we still need to consider aggregator's DB boundness because it can be error boundness. 
		if (combinedExplicitAndMetadataBoundness is DbBoundnessType.NotDefined or DbBoundnessType.Error)
		{
			calcedBoundnessByAttributeApplication[attributeApplication] = combinedExplicitAndMetadataBoundness;
			return combinedExplicitAndMetadataBoundness;
		}

		// Non-aggregated attribute, can be an aggregator. No need to consider aggregator corrections for DB boundness
		if (!attributeApplication.HasAggregator)    
		{
			calcedBoundnessByAttributeApplication[attributeApplication] = combinedExplicitAndMetadataBoundness;
			return combinedExplicitAndMetadataBoundness;
		}

		// Aggregated attributes, can be aggregators themselves
		var aggregatorDbBoundness = recursionDepth > MaxRecursion
			? DbBoundnessType.NotDefined
			: CalculateDbBoundnessForAttributeFromMetadataAndApplication(attributeApplication.Aggregator, calcedBoundnessByAttributeApplication, 
																		 attributeDBBoundnessFromMetadataByAttributeType, recursionDepth + 1);
		var combinedBoundnessFromApplicationAndMetadataAndAggregator = 
			DbBoundnessTypeCombiner.CombineBoundnessFromAggregatorAndAggregatedAttributes(combinedExplicitAndMetadataBoundness,
																						  aggregatorDbBoundness);

		calcedBoundnessByAttributeApplication[attributeApplication] = combinedBoundnessFromApplicationAndMetadataAndAggregator;
		return combinedBoundnessFromApplicationAndMetadataAndAggregator;
	}

	private static DbBoundnessType CombineExplicitlySetAndMetadataBoundness(DbBoundnessType explicitDbBoundnessFromApplication,
																			DbBoundnessType dbBoundnessFromMetadata)
	{
		if (dbBoundnessFromMetadata == DbBoundnessType.NotDefined)  // shortcut for undefined metadata boundness
			return explicitDbBoundnessFromApplication;

		// Custom combination rules, explicitly set DB boundness takes priority over the DB boundness from metadata in Unbound/DbBound/Unknown cases.
		// Important to remember that dbBoundnessFromMetadata is not Error here because that case was already handled earlier.
		// It also shouldn't be PXDBCalced or PXDBScalar because such DB boundness can't be set explicitly.
		switch (explicitDbBoundnessFromApplication)
		{
			case DbBoundnessType.Unbound:
			case DbBoundnessType.DbBound:
				return dbBoundnessFromMetadata switch
				{
					// normal boundness resolution
					DbBoundnessType.PXDBScalar => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplication),
					DbBoundnessType.PXDBCalced => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplication),
					DbBoundnessType.Error 	   => DbBoundnessType.Error,
					// Explicit DB boundness takes priority over Unbound/DbBound/Unknown from metadata
					_						   => explicitDbBoundnessFromApplication
				};

			// dbBoundnessFromMetadata can be at maximum Unknown here. Thus it's safe to say that explicitDbBoundnessFromApplication will override it
			case DbBoundnessType.Unknown:
			case DbBoundnessType.Error:
				return explicitDbBoundnessFromApplication;

			// PXDBCalced or PXDBScalar can't be set explicitly on the attribute's declaration. 
			// So this case indicates some error with the analysis state or DB boundness configuration.
			case DbBoundnessType.PXDBScalar:
			case DbBoundnessType.PXDBCalced:
				return DbBoundnessType.Error;

			case DbBoundnessType.NotDefined:
			default:
				return dbBoundnessFromMetadata;
		}
	}
}
