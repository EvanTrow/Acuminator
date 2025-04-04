using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.PXFieldAttributes.Enum;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacFieldAndReferencedFieldMismatch
{
	public class DacFieldAndReferencedFieldMismatchAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1078_TypesOfDacFieldAndReferencedFieldMismatch,
				Descriptors.PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize);

		private static readonly SpecialType[] TypesToExcludeFromAnalysis = 
		{
			SpecialType.System_Boolean, 
			SpecialType.System_Decimal, 
			SpecialType.System_DateTime,
			SpecialType.System_Double, 
			SpecialType.System_Single, 
			SpecialType.System_Array, 
			SpecialType.System_Object
		};

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var propertiesToCheck = dac.DeclaredDacFieldPropertiesWithAcumaticaAttributes
									   .Where(propertyInfo => !TypesToExcludeFromAnalysis.Contains(propertyInfo.PropertyTypeUnwrappedNullable.SpecialType))
									   .Select(propertyInfo => (Property: propertyInfo, ForeignRefAttribute: TryGetForeignReferenceAttribute(pxContext, propertyInfo)))
									   .Where(p => p.ForeignRefAttribute is not null);

			FieldTypeAttributesMetadataProvider? fieldTypeAttributesMetadataProvider = null;
			var foreignDacPropertyInfoRetriever = new ForeignDacPropertyInfoRetriever(pxContext, context.CancellationToken);

			foreach (var (property, foreignReferenceAttribute) in propertiesToCheck)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				fieldTypeAttributesMetadataProvider ??= new FieldTypeAttributesMetadataProvider(pxContext);
				CheckPropertyWithForeignAttribute(context, property, foreignReferenceAttribute!, dac, pxContext, 
												  foreignDacPropertyInfoRetriever, fieldTypeAttributesMetadataProvider);
			}
		}

		private static DacFieldAttributeInfo? TryGetForeignReferenceAttribute(PXContext pxContext, DacPropertyInfo property) =>
			property.Attributes.FirstOrDefault(x => ContainsAttributeMatching(x, pxContext.AttributeTypes.PXSelectorAttribute.Type));

		private void CheckPropertyWithForeignAttribute(SymbolAnalysisContext context, DacPropertyInfo property, DacFieldAttributeInfo foreignReferenceAttribute,
													   DacSemanticModel dac, PXContext pxContext, ForeignDacPropertyInfoRetriever foreignDacPropertyInfoRetriever,
													   FieldTypeAttributesMetadataProvider fieldTypeAttributesMetadataProvider)
		{
			var foreignDacProperty = foreignDacPropertyInfoRetriever.GetForeignDacPropertyInfo(foreignReferenceAttribute);

			if (foreignDacProperty is null)
				return;

			var foreignDacPropertyType = foreignDacProperty.PropertyType;

			if (HaveDifferentUnderlyingType(property, foreignDacPropertyType))
				ReportTypeMismatch(context, pxContext, property, foreignDacProperty);
			else
			{
				var localTypeAttributes = property.Attributes.Where(
					x => ContainsAttributeMatching(x, pxContext.FieldAttributes.PXDBFieldAttribute)
					|| ContainsAttributeMatchingOneOf(x, fieldTypeAttributesMetadataProvider.UnboundDacFieldTypeAttributesWithFieldType.Keys))
					.ToList(property.Attributes.Length);

				if (localTypeAttributes.Count == 0)
					return;

				if (PropertyTypesAreIncompatible(property)
					|| PropertyTypesAreIncompatible(foreignDacProperty))
					return;

				var foreignFieldSizes = foreignDacProperty!.Attributes
					.Where(
						x => ContainsAttributeMatching(x, pxContext.FieldAttributes.PXDBFieldAttribute)
						|| ContainsAttributeMatchingOneOf(x, fieldTypeAttributesMetadataProvider.UnboundDacFieldTypeAttributesWithFieldType.Keys))
					.Select(x => GetFieldSize(x, pxContext))
					.OfType<int>()
					.ToList();

				// agreed to stop checking property here as the code may be incomplete at this point
				if (foreignFieldSizes.Count != 1)
					return;

				var foreignFieldSize = foreignFieldSizes[0];

				var attributesWithSizeMismatch = localTypeAttributes.Select(x => (x, GetFieldSize(x, pxContext)))
																	.Where(x => x.Item2 is not null && x.Item2 != foreignFieldSize);

				foreach (var (attribute, size) in attributesWithSizeMismatch)
				{
					ReportTypeSizeMismatch(
						context, pxContext, attribute, property.Name,
						foreignDacProperty.Name, foreignFieldSize);
				}
			}
		}

		private static bool PropertyTypesAreIncompatible(DacPropertyInfo property)
		{
			var attributesWithFieldTypeMetadata = property.Attributes
														  .Where(aInfo => !aInfo.AggregatedAttributeMetadata.IsDefaultOrEmpty)
														  .ToList(capacity: property.Attributes.Length);
			var (typeAttributesOnDacProperty, typeAttributesWithDifferentDataTypesOnAggregator, _) =
				attributesWithFieldTypeMetadata.FilterTypeAttributes();

			if (typeAttributesWithDifferentDataTypesOnAggregator?.Count > 0)
				return false;

			var compatibility = property.CheckCompatibility(typeAttributesOnDacProperty![0]);

			return compatibility != TypesCompatibility.CompatibleTypes;
		}

		private static bool HaveDifferentUnderlyingType(DacPropertyInfo localProperty, ITypeSymbol foreignPropertyType) =>
			!SymbolEqualityComparer.Default.Equals(localProperty.PropertyType, foreignPropertyType);

		private static int? GetFieldSize(DacFieldAttributeInfo attrInfo, PXContext pxContext)
		{
			var lengthCtorArguments = attrInfo.FlattenedAcumaticaAttributes
					.Select(x => x.Application)
					.Select(attr => GetLengthConstructorArgument(attr, pxContext))
					.OfType<int>()
					.Distinct()
					.ToList(attrInfo.FlattenedAcumaticaAttributes.Count);

			bool hasLength = lengthCtorArguments?.Count > 0;

			return lengthCtorArguments?.Count switch
			{
				0 or null => -1,
				1 => lengthCtorArguments[0],
				_ => null
			};
		}

		private static int? GetLengthConstructorArgument(AttributeData attributeData, PXContext pxContext)
		{
			if (attributeData.AttributeClass is null)
				return null;

			if (pxContext.FieldAttributes.DataAttributesWithHardcodedLength
					.TryGetValue(attributeData.AttributeClass, out var length))
				return length;

			if (!IsTypeWithLength(attributeData, pxContext))
				return null;

			if (attributeData.AttributeConstructor?.Parameters.Length is null or 0)
				return null;

			var index = attributeData.AttributeConstructor.Parameters
				.FindIndex(parameter =>
					parameter.Type.SpecialType == SpecialType.System_Int32
					&& (parameter.Name.Equals("length", StringComparison.OrdinalIgnoreCase)
					|| parameter.Name.Equals("size", StringComparison.OrdinalIgnoreCase)));

			if (index == -1 || attributeData.ConstructorArguments.Length <= index) return null;

			return attributeData.ConstructorArguments[index].Value is int i ? i : null;
		}

		private static bool IsTypeWithLength(AttributeData attribute, PXContext pxContext) =>
			pxContext.FieldAttributes.DataAttributesWithLength.Contains<INamedTypeSymbol>(
				attribute.AttributeClass!,
				SymbolEqualityComparer.Default);

		private static bool ContainsAttributeMatching(DacFieldAttributeInfo attrInfo, ITypeSymbol expectedType) =>
			attrInfo.FlattenedAcumaticaAttributes.Any(attr => attr.Type.Equals(expectedType, SymbolEqualityComparer.Default));

		private static bool ContainsAttributeMatchingOneOf(DacFieldAttributeInfo attrInfo, IEnumerable<ITypeSymbol> types) =>
			attrInfo.FlattenedAcumaticaAttributes.Select(x => x.Type).Intersect(types, SymbolEqualityComparer.Default).Any();

		private static void ReportTypeMismatch(SymbolAnalysisContext context, PXContext pxContext, DacPropertyInfo property,
			DacPropertyInfo referencedProperty)
		{
			var diagnostic = Diagnostic.Create(
				Descriptors.PX1078_TypesOfDacFieldAndReferencedFieldMismatch,
				property.Symbol.Locations.FirstOrDefault(),
				property.Name,
				referencedProperty.Name,
				referencedProperty.PropertyType.Name);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static void ReportTypeSizeMismatch(SymbolAnalysisContext context, PXContext pxContext, DacFieldAttributeInfo localDeclaration,
			string localName, string externalName, int expectedLength)
		{
			var location = localDeclaration.AttributeData.GetLocation(context.CancellationToken);
			var diagnostic = Diagnostic.Create(
				Descriptors.PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize,
				location, localName, externalName, expectedLength);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
