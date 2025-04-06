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
									   .Select(propertyInfo => (Property: propertyInfo, 
																ForeignRefAttribute: TryGetForeignReferenceAttribute(pxContext, propertyInfo)))
									   .Where(p => p.ForeignRefAttribute is not null);

			var foreignDacPropertyInfoRetriever = new ForeignDacPropertyInfoRetriever(pxContext, context.CancellationToken);

			foreach (var (property, foreignReferenceAttribute) in propertiesToCheck)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				CheckPropertyWithForeignAttribute(context, property, foreignReferenceAttribute!, dac, pxContext, 
												  foreignDacPropertyInfoRetriever);
			}
		}

		private static DacFieldAttributeInfo? TryGetForeignReferenceAttribute(PXContext pxContext, DacPropertyInfo property) =>
			property.Attributes.FirstOrDefault(attrInfo => attrInfo.AggregatesAttribute(pxContext.AttributeTypes.PXSelectorAttribute.Type));

		private void CheckPropertyWithForeignAttribute(SymbolAnalysisContext context, DacPropertyInfo localDacProperty,
													   DacFieldAttributeInfo foreignReferenceAttribute, DacSemanticModel dac, PXContext pxContext,
													   ForeignDacPropertyInfoRetriever foreignDacPropertyInfoRetriever)
		{
			var foreignDacProperty = foreignDacPropertyInfoRetriever.GetForeignDacPropertyInfo(foreignReferenceAttribute);

			if (foreignDacProperty is null)
				return;

			var foreignDacPropertyType = foreignDacProperty.PropertyType;

			if (ArePropertyTypesDifferent(localDacProperty, foreignDacPropertyType))
			{
				ReportTypeMismatch(context, pxContext, localDacProperty, foreignDacProperty);
				return;
			}

			if (!localDacProperty.HasNonNullDataType ||
				!IsPropertyTypeCompatibleWithDataTypeAttributes(localDacProperty) ||
				!IsPropertyTypeCompatibleWithDataTypeAttributes(foreignDacProperty))
			{
				return;
			}

			var foreignFieldSizes = foreignDacProperty!.DeclaredDataTypeAttributes
													   .AllDeclaredDatatypeAttributesOnDacProperty
													   .Select(dataTypeAttr => GetFieldSize(dataTypeAttr, pxContext))
													   .OfType<int>()
													   .ToList();

			// agreed to stop checking property here as the code may be incomplete at this point
			if (foreignFieldSizes.Count != 1)
				return;

			var foreignFieldSize = foreignFieldSizes[0];

			var attributesWithSizeMismatch = foreignDacProperty!.DeclaredDataTypeAttributes
													   .AllDeclaredDatatypeAttributesOnDacProperty.Select(x => (x, GetFieldSize(x, pxContext)))
																				  .Where(x => x.Item2 is not null && x.Item2 != foreignFieldSize);

			foreach (var (attribute, size) in attributesWithSizeMismatch)
			{
				ReportTypeSizeMismatch(
					context, pxContext, attribute, localDacProperty.Name,
					foreignDacProperty.Name, foreignFieldSize);
			}
		}

		private static bool ArePropertyTypesDifferent(DacPropertyInfo localDacProperty, ITypeSymbol foreignPropertyType) =>
			!SymbolEqualityComparer.Default.Equals(localDacProperty.PropertyType, foreignPropertyType);

		private static bool IsPropertyTypeCompatibleWithDataTypeAttributes(DacPropertyInfo property)
		{
			if (!property.DeclaredDataTypeAttributes.DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes.IsDefaultOrEmpty)
				return false;
			
			return property.EffectivePropertyAndDataTypeAttributeTypesCompatibility == 
				   CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.CompatibleTypes;
		}

		private static int? GetFieldSize(DacFieldAttributeInfo declaredDataTypeAttribute, PXContext pxContext)
		{
			var lengthCtorArguments = 
				declaredDataTypeAttribute.FlattenedAcumaticaAttributes
										 .Select(aggregatedAttr => GetLengthConstructorArgument(aggregatedAttr.Application, pxContext))
										.OfType<int>()
										.Distinct()
										.ToList(declaredDataTypeAttribute.FlattenedAcumaticaAttributes.Count);

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
