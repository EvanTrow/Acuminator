#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to retrieve the information about DB boundness of concrete application of Acumatica attribute.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class DbBoundnessCalculator
	{
		public PXContext Context { get; }

		public FieldTypeAttributesMetadataProvider AttributesMetadataProvider { get; }

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull();
			AttributesMetadataProvider = new FieldTypeAttributesMetadataProvider(pxContext);
		}

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		public DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication) =>
			GetAttributeApplicationDbBoundnessType(attributeApplication, preparedFlattenedAttributesWithApplications: null, 
												   preparedFlattenedAttributesSet: null, preparedAttributesMetadata: null);

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <param name="preparedFlattenedAttributesWithApplications">
		/// The optional already prepared flattened attributes with applications,<br/>
		/// the result of the <see cref="AcumaticaAttributesRelationsInfoProvider.GetThisAndAllAggregatedAttributesWithApplications(AttributeData?, PXContext, bool)"/> call.<br/>
		/// If <see langword="null"/> then the flattened attributes with applications set will be calculated.
		/// </param>
		/// <param name="preparedFlattenedAttributesSet">The prepared flattened attributes set. If <see langword="null"/> then the flattened attributes set will be calculated.</param>
		/// <param name="preparedAttributesMetadata">
		/// The prepared attribute aggregated metadata, the result of the <see cref="FieldTypeAttributesMetadataProvider.GetDacFieldTypeAttributeInfos(ITypeSymbol)"/>
		/// call. If <see langword="null"/> then the aggregated metadata will be calculated.
		/// </param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		internal DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication, 
																		ImmutableHashSet<AttributeWithApplicationAndAggregator>? preparedFlattenedAttributesWithApplications,
																		ISet<ITypeSymbol>? preparedFlattenedAttributesSet,
																		IReadOnlyCollection<DataTypeAttributeInfo>? preparedAttributesMetadata)
		{
			attributeApplication.ThrowOnNull();

			if (attributeApplication.AttributeClass == null || !attributeApplication.AttributeClass.IsAcumaticaAttribute(Context))
				return DbBoundnessType.NotDefined;

			// First, check if the attribute is present in the set of known non-data-type attributes or is derived from them
			if (AttributesMetadataProvider.IsWellKnownNonDataTypeAttribute(attributeApplication.AttributeClass))
				return DbBoundnessType.NotDefined;

			var flattenedAttributesWithApplications = preparedFlattenedAttributesWithApplications ??
													  attributeApplication.GetThisAndAllAggregatedAttributesWithApplications(Context, includeBaseTypes: true);

			if (flattenedAttributesWithApplications.Count == 0)
				return ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication);

			// Check combined information from attribute applications and metadata
			var flattenedAttributesSet = preparedFlattenedAttributesSet ?? 
										 flattenedAttributesWithApplications.Select(atrWithApp => atrWithApp.Type)
																			.ToHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
			var attributesMetadata = 
				preparedAttributesMetadata ?? 
				AttributesMetadataProvider.GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(attributeApplication.AttributeClass,
																											   flattenedAttributesSet);
			bool checkForExplicitlySetDbBoundnessOnDirectAttributeApplication =
				attributesMetadata.Count > 0 &&
				attributesMetadata.Any(attributeInfo => attributeInfo.Kind == FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute);

			if (checkForExplicitlySetDbBoundnessOnDirectAttributeApplication)
			{
				var dbBoundnessOnApplication = ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication);

				if (dbBoundnessOnApplication != DbBoundnessType.NotDefined)
					return dbBoundnessOnApplication;
			}

			DbBoundnessType combinedDbBoundness = GetDbBoundnessFromAttributesApplicationsAndMetadata(flattenedAttributesWithApplications, attributesMetadata);

			if (combinedDbBoundness != DbBoundnessType.NotDefined)
				return combinedDbBoundness;

			return DuckTypingCheckIfAttributeHasMixedDbBoundness(flattenedAttributesSet);
		}

		private DbBoundnessType GetDbBoundnessFromAttributesApplicationsAndMetadata(ImmutableHashSet<AttributeWithApplicationAndAggregator> flattenedAttributesWithApplications,
																					IReadOnlyCollection<DataTypeAttributeInfo> attributesMetadata)
		{
			if (attributesMetadata.Count == 0)
			{
				return flattenedAttributesWithApplications
							.Select(attrWithApplication => GetDbBoundnessSetExplicitlyByAttributeApplication(attrWithApplication.Application))
							.Combine();
			}

			var applicationsByAttribute = flattenedAttributesWithApplications
											.ToLookup(attrAppl => attrAppl.Type,
													  SymbolEqualityComparer.Default as IEqualityComparer<ITypeSymbol>);
			var combinedBoundness =
				attributesMetadata.Select(attributeInfo => GetDbBoundnessFromMetadataAndApplications(attributeInfo, applicationsByAttribute))
								  .Combine();
			return combinedBoundness;
		}

		private DbBoundnessType GetDbBoundnessFromMetadataAndApplications(DataTypeAttributeInfo attributeInfo, 
																		  ILookup<ITypeSymbol, AttributeWithApplicationAndAggregator> applicationsByAttribute)
		{
			// Even if the DB boundness is set explicitly at the application we have to calculate DB boundness from metadata
			// Because it can be inconsistent in which case explicitly set boundness can't apply
			DbBoundnessType dbBoundnessFromMetadata = GetDbBoundnessFromAttributeInfo(attributeInfo);

			if (dbBoundnessFromMetadata == DbBoundnessType.Error)
				return dbBoundnessFromMetadata;

			// For non-mixed DB boundness attributes there is nothing more to apply
			if (attributeInfo is not MixedDbBoundnessAttributeInfo mixedDbBoundnessAttributeInfo || 
				!applicationsByAttribute.Contains(mixedDbBoundnessAttributeInfo.AttributeType))
			{
				return dbBoundnessFromMetadata;
			}

			var attributeApplications = applicationsByAttribute[mixedDbBoundnessAttributeInfo.AttributeType];
			var explicitDbBoundnessFromApplications = 
				attributeApplications.Select(attrWithAppl => ExplicitlySetAttributeDbBoundnessCalculator.GetDbBoundnessSetExplicitlyByAttributeApplication(
																attrWithAppl.Application))
									 .Combine();

			var combinedBoundness = CombineExplictlySetAndMetadataBoundnesses(explicitDbBoundnessFromApplications, dbBoundnessFromMetadata);
			return combinedBoundness;
		}

		private DbBoundnessType GetDbBoundnessFromAttributeInfo(DataTypeAttributeInfo attributeInfo)
		{
			switch (attributeInfo.Kind)
			{
				case FieldTypeAttributeKind.BoundTypeAttribute:
					return DbBoundnessType.DbBound;
				case FieldTypeAttributeKind.UnboundTypeAttribute:
					return DbBoundnessType.Unbound;
				case FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute:
					if (attributeInfo is not MixedDbBoundnessAttributeInfo mixedDbBoundnessAttributeInfo ||
						!mixedDbBoundnessAttributeInfo.IsDbBoundByDefault.HasValue)
					{
						return DbBoundnessType.Unknown;
					}

					return mixedDbBoundnessAttributeInfo.IsDbBoundByDefault.Value
						? DbBoundnessType.DbBound
						: DbBoundnessType.Unbound;

				case FieldTypeAttributeKind.PXDBScalarAttribute:
					return DbBoundnessType.PXDBScalar;
				case FieldTypeAttributeKind.PXDBCalcedAttribute:
					return DbBoundnessType.PXDBCalced;
				default:
					return DbBoundnessType.NotDefined;
			}
		}

		private DbBoundnessType CombineExplictlySetAndMetadataBoundnesses(DbBoundnessType explicitDbBoundnessFromApplications, 
																		  DbBoundnessType dbBoundnessFromMetadata)
		{
			// Custom combination rules
			switch (explicitDbBoundnessFromApplications)
			{
				case DbBoundnessType.Unbound:
					return dbBoundnessFromMetadata switch
					{
						// normal boundness resolution
						DbBoundnessType.PXDBCalced or DbBoundnessType.PXDBScalar
						or DbBoundnessType.Error or DbBoundnessType.NotDefined	 => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications),
						// explicit boundndess takes priority
						DbBoundnessType.Unbound or DbBoundnessType.DbBound or 
						DbBoundnessType.Unknown									 => explicitDbBoundnessFromApplications,
						_														 => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications)
					};
					
				case DbBoundnessType.DbBound:
					return dbBoundnessFromMetadata is DbBoundnessType.PXDBScalar or DbBoundnessType.PXDBCalced or DbBoundnessType.Error
						? dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications)
						: explicitDbBoundnessFromApplications;
					
				case DbBoundnessType.Unknown:
				case DbBoundnessType.Error:
					return explicitDbBoundnessFromApplications;

				case DbBoundnessType.NotDefined:
				default:
					return dbBoundnessFromMetadata;
			}
		}

		/// <summary>
		/// Duck typing check if attribute Has mixed database boundness. 
		/// The check will look for a presence of IsDBField and NonDB properties on the flattened Acumatica attributes set of the checked attribute.<br/>
		/// If there is a suitable property - return <see cref="DbBoundnessType.Unknown"/> since we can only spot the known pattern but can't deduce
		/// attribute's DB boundness. 
		/// </summary>
		/// <param name="flattenedAttributesSet">
		/// Flattened Acumatica attributes of the checked attribute which includes aggregated attributes, aggregates on aggregates and their base types.
		/// </param>
		/// <returns>
		/// DBBoundness deduced by the duck typing.
		/// </returns>
		private DbBoundnessType DuckTypingCheckIfAttributeHasMixedDbBoundness(IEnumerable<ITypeSymbol> flattenedAttributesSet)
		{
			//only IsDBField and NonDB properties are considered in analysis for attributes that can be applied to both bound and unbound fields
			foreach (var attributeType in flattenedAttributesSet)
			{
				var members = attributeType.GetMembers();

				if (members.IsDefaultOrEmpty)
					continue;

				var hasIsDbFieldOrNonDbProperties = members.OfType<IPropertySymbol>()
													.Any(property => !property.IsStatic && property.IsExplicitlyDeclared() && 
																	  property.IsDeclaredInType(attributeType) &&
																			(Constants.IsDBField.Equals(property.Name, StringComparison.OrdinalIgnoreCase) ||
																			 Constants.NonDB.Equals(property.Name, StringComparison.OrdinalIgnoreCase)));
				if (hasIsDbFieldOrNonDbProperties)
					return DbBoundnessType.Unknown;
			}

			return DbBoundnessType.NotDefined;
		}
	}
}