using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about attributes of a DAC field.
	/// </summary>
	public class DacFieldAttributeInfo : AttributeInfoBase
	{
		private readonly ISet<ITypeSymbol> _flattenedAttributeTypes;

		public override AttributePlacement Placement => AttributePlacement.DacField;

		/// <summary>
		/// The flattened Acumatica attributes with the application set: the current attribute, its base attributes, 
		/// aggregated attributes in case of an aggregate attribute, 
		/// aggregates on aggregates and so on.
		/// </summary>
		public ImmutableHashSet<AttributeWithApplication> FlattenedAcumaticaAttributes { get; }

		/// <summary>
		/// The aggregated attribute metadata collection which is information from the flattened attributes set. 
		/// This information is mostly related to the attribute's relationship with the database.
		/// </summary>
		public ImmutableArray<DataTypeAttributeInfo> AggregatedAttributeMetadata { get; }

		public DbBoundnessType DbBoundness { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsDefaultAttribute { get; }

		public bool IsAutoNumberAttribute { get; }

		public bool IsAcumaticaAttribute { get; }

		protected DacFieldAttributeInfo(PXContext pxContext, AttributeData attributeData, IEnumerable<AttributeWithApplication> flattenedAttributeApplications,
										ISet<ITypeSymbol> flattenedAttributeTypes, IEnumerable<DataTypeAttributeInfo> attributeInfos,
										DbBoundnessType dbBoundness, int declarationOrder) :
								   base(attributeData, declarationOrder)
		{
			_flattenedAttributeTypes 	 = flattenedAttributeTypes.CheckIfNull();
			FlattenedAcumaticaAttributes = (flattenedAttributeApplications as ImmutableHashSet<AttributeWithApplication>) ??
											flattenedAttributeApplications.ToImmutableHashSet();
			AggregatedAttributeMetadata  = attributeInfos.ToImmutableArray();
			DbBoundness 				 = dbBoundness;

			IsKey 				  = attributeData.NamedArguments.Any(arg => arg.Key.Contains(PropertyNames.Attributes.IsKey) &&
																			arg.Value.Value is bool isKeyValue && isKeyValue);
			IsIdentity 			  = IsDerivedFromIdentityTypes(_flattenedAttributeTypes, pxContext);
			IsDefaultAttribute	  = IsPXDefaultAttribute(_flattenedAttributeTypes, pxContext);
			IsAutoNumberAttribute = CheckForAutoNumberAttribute(_flattenedAttributeTypes, pxContext);
			IsAcumaticaAttribute  = IsAcumaticaFrameworkAttribute(_flattenedAttributeTypes, attributeData.AttributeClass, pxContext);
		}

		public bool AggregatesAttribute(ITypeSymbol? attributeTypeToCheck) =>
			attributeTypeToCheck != null && _flattenedAttributeTypes.Contains(attributeTypeToCheck);

		public bool AggregatesOneOfAttributes(IEnumerable<ITypeSymbol> attributeTypesToCheck) =>
			attributeTypesToCheck.CheckIfNull().Any(AggregatesAttribute);

		public static DacFieldAttributeInfo Create(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder) =>
			CreateUnsafe(attribute.CheckIfNull(), dbBoundnessCalculator.CheckIfNull(), declarationOrder);

		public static DacFieldAttributeInfo CreateUnsafe(AttributeData attribute, DbBoundnessCalculator dbBoundnessCalculator, int declarationOrder)
		{
			var flattenedAttributeApplications = attribute.GetThisAndAllAggregatedAttributesWithApplications(
																dbBoundnessCalculator.Context, includeBaseTypes: true);
			ISet<ITypeSymbol> flattenedAttributeTypes = flattenedAttributeApplications.Count > 0
				? flattenedAttributeApplications.Select(attributeWithApplication => attributeWithApplication.Type)
												.ToHashSet<ITypeSymbol>(SymbolEqualityComparer.Default)
				: ImmutableHashSet<ITypeSymbol>.Empty;

			var aggregatedMetadata = dbBoundnessCalculator.AttributesMetadataProvider
														  .GetDacFieldTypeAttributeInfos(attribute.AttributeClass, flattenedAttributeTypes);
			DbBoundnessType dbBoundness = dbBoundnessCalculator.GetAttributeApplicationDbBoundnessType(attribute, flattenedAttributeApplications, 
																										flattenedAttributeTypes, aggregatedMetadata);

			return new DacFieldAttributeInfo(dbBoundnessCalculator.Context, attribute, flattenedAttributeApplications, flattenedAttributeTypes, 
											 aggregatedMetadata, dbBoundness,declarationOrder);
		}

		private static bool IsDerivedFromIdentityTypes(ISet<ITypeSymbol> flattenedAttributes, PXContext pxContext) =>
			flattenedAttributes.Contains(pxContext.FieldAttributes.PXDBIdentityAttribute) ||
			flattenedAttributes.Contains(pxContext.FieldAttributes.PXDBLongIdentityAttribute);

		private static bool IsPXDefaultAttribute(ISet<ITypeSymbol> flattenedAttributes, PXContext pxContext) =>
			flattenedAttributes.Contains(pxContext.AttributeTypes.PXDefaultAttribute) && 
			!flattenedAttributes.Contains(pxContext.AttributeTypes.PXUnboundDefaultAttribute);
		
		private static bool CheckForAutoNumberAttribute(ISet<ITypeSymbol> flattenedAttributes, PXContext pxContext)
		{
			var autoNumberAttribute = pxContext.AttributeTypes.AutoNumberAttribute.Type;

			if (autoNumberAttribute == null)
				return false;

			return flattenedAttributes.Contains(autoNumberAttribute);
		}

		private static bool IsAcumaticaFrameworkAttribute(ISet<ITypeSymbol> flattenedAttributes, INamedTypeSymbol? attributeType, PXContext pxContext) =>
			flattenedAttributes.Count > 0 || pxContext.AttributeTypes.PXEventSubscriberAttribute.Equals(attributeType, SymbolEqualityComparer.Default);
	}
}