using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes.Enum;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
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

			if (!localDacProperty.HasNonNullDataType || !foreignDacProperty.HasNonNullDataType ||
				!IsPropertyTypeCompatibleWithDataTypeAttributes(localDacProperty) ||
				!IsPropertyTypeCompatibleWithDataTypeAttributes(foreignDacProperty))
			{
				return;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			DacFieldSize foreignFieldSize = foreignDacProperty.GetFieldSize(pxContext);

			// Stop checking the current DAC field, since we fail to determine the size of its corresponding foreign DAC field
			if (foreignFieldSize.IsInconsistent)
				return;

			var attributesWithSizeMismatch = 
				localDacProperty!.DeclaredDataTypeAttributes
								 .AllDeclaredDatatypeAttributesOnDacProperty
								 .Select(attr => (Attribute: attr, FieldSize: attr.GetFieldSize(pxContext)))
								 .Where(pair => !pair.FieldSize.IsInconsistent &&			//Do not check attributes with inconsistent sizes, they should be checked by PX1023
												!pair.FieldSize.Equals(foreignFieldSize));

			foreach (var (dataTypeAttribute, localFieldSize) in attributesWithSizeMismatch)
			{
				ReportTypeSizeMismatch(context, pxContext, dataTypeAttribute, localDacProperty.Name,
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

		private static void ReportTypeSizeMismatch(SymbolAnalysisContext context, PXContext pxContext, 
												   DacFieldAttributeInfo localDataTypeAtributeToReport,
												   string localDacPropertyName, string foreignDacPropertyName, 
												   DacFieldSize foreignDacFieldSize)
		{
			var location = localDataTypeAtributeToReport.AttributeData.GetLocation(context.CancellationToken)
																	  .NullIfLocationKindIsNone();
			if (location is null)
				return;

			string expectedFieldSize = foreignDacFieldSize.IsNotDefined
				? "not defined"
				: foreignDacFieldSize.Value.ToString();

			var diagnostic = Diagnostic.Create(Descriptors.PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize,
											   location, localDacPropertyName, foreignDacPropertyName, expectedFieldSize);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
