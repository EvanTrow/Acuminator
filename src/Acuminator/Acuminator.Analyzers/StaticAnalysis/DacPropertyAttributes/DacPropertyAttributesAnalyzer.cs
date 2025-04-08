
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	public class DacPropertyAttributesAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty,
				Descriptors.PX1023_MultipleTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators,
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty,
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators,
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute,
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			foreach (DacPropertyInfo property in dacOrDacExt.AllDeclaredProperties)
			{
				CheckDacProperty(property, context, pxContext);
			}	
		}

		private static void CheckDacProperty(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<DacFieldAttributeInfo> attributes = property.Attributes;

			if (attributes.IsDefaultOrEmpty)
				return;

			var attributesWithFieldTypeMetadata = attributes.Where(aInfo => !aInfo.AggregatedAttributeMetadata.IsDefaultOrEmpty)
															.ToList(capacity: attributes.Length);
			if (attributesWithFieldTypeMetadata.Count == 0)
				return;

			bool validAttributesCalcedOnDbSide = CheckForMultipleAttributesCalcedOnDbSide(symbolContext, pxContext, property, 
																						  attributesWithFieldTypeMetadata);
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!validAttributesCalcedOnDbSide)
				return;

			CheckForCalcedOnDbSideAndUnboundTypeAttributes(symbolContext, pxContext, property, attributesWithFieldTypeMetadata);
			CheckDacFieldDataTypeAttributes(property, symbolContext, pxContext, attributesWithFieldTypeMetadata);
		}
	
		private static bool CheckForMultipleAttributesCalcedOnDbSide(SymbolAnalysisContext symbolContext, PXContext pxContext,
																	 DacPropertyInfo property, List<DacFieldAttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// Optimization - properties with the following DB boudnesses defintely don't have attributes calced on DB side
			if (property.EffectiveDbBoundness is DbBoundnessType.DbBound or DbBoundnessType.Unbound or DbBoundnessType.NotDefined)
				return true;

			var (attributesCalcedOnDbSideDeclaredOnDacProperty, attributesCalcedOnDbSideWithConflictingAggregatorDeclarations) =
				FilterAttributeInfosCalcedOnDbSide();

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.IsNullOrEmpty() ||
				(attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 1 && attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.IsNullOrEmpty()))
			{
				return true;
			}

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty);
			}

			if (attributesCalcedOnDbSideWithConflictingAggregatorDeclarations?.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<DacFieldAttributeInfo>?, List<DacFieldAttributeInfo>?) FilterAttributeInfosCalcedOnDbSide()
			{
				List<DacFieldAttributeInfo>? attributesCalcedOnDbSideOnDacProperty = null;
				List<DacFieldAttributeInfo>? attributesCalcedOnDbSideInvalidAggregatorDeclarations = null;

				foreach (var attribute in attributesWithFieldTypeMetadata)
				{
					int counterOfCalcedOnDbSideAttributeInfos = 0;
					var dbCalcedFieldTypeAttributes = attribute.AggregatedAttributeMetadata.Where(atrMetadata => atrMetadata.IsCalculatedOnDbSide);

					foreach (var dbCalcedAttributeInfo in dbCalcedFieldTypeAttributes)
					{
						counterOfCalcedOnDbSideAttributeInfos++;

						if (counterOfCalcedOnDbSideAttributeInfos > 1)
							break;
					}

					if (counterOfCalcedOnDbSideAttributeInfos > 0)
					{
						attributesCalcedOnDbSideOnDacProperty ??= new List<DacFieldAttributeInfo>(capacity: 2);
						attributesCalcedOnDbSideOnDacProperty.Add(attribute);
					}

					if (counterOfCalcedOnDbSideAttributeInfos > 1)
					{
						attributesCalcedOnDbSideInvalidAggregatorDeclarations ??= new List<DacFieldAttributeInfo>(capacity: 2);
						attributesCalcedOnDbSideInvalidAggregatorDeclarations.Add(attribute);
					}
				}

				return (attributesCalcedOnDbSideOnDacProperty, attributesCalcedOnDbSideInvalidAggregatorDeclarations);
			}
		}

		private static void CheckForCalcedOnDbSideAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
																		   DacPropertyInfo property, List<DacFieldAttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// Optimization - properties with the following DB boudnesses defintely don't have attributes calced on DB side
			if (property.EffectiveDbBoundness is DbBoundnessType.DbBound or DbBoundnessType.Unbound or DbBoundnessType.NotDefined)
				return;

			bool hasPXDBCalcedAttribute = false, hasPXDBScalarAttribute = false, hasUnboundTypeAttribute = false;

			foreach (var attrInfo in attributesWithFieldTypeMetadata)
			{
				switch (attrInfo.DbBoundness)
				{
					case DbBoundnessType.Unbound:
						hasUnboundTypeAttribute = true;
						continue;	
					case DbBoundnessType.PXDBScalar:
						hasPXDBScalarAttribute = true;
						continue;
					case DbBoundnessType.PXDBCalced:
						hasPXDBCalcedAttribute = true;
						continue;			
				}
			}

			// Node not null here because aggregated DAC analysers by default run only on DACs in source 
			// and these properties are declared in the DAC type itself
			if (hasUnboundTypeAttribute || (!hasPXDBCalcedAttribute && !hasPXDBScalarAttribute) ||
				property.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() is not Location location)
			{
				return;
			}

			if (hasPXDBCalcedAttribute)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, location);
				symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}

			if (hasPXDBScalarAttribute)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute, location);
				symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private static void CheckDacFieldDataTypeAttributes(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
															List<DacFieldAttributeInfo> attributesWithFieldTypeMetadata)
		{
			if (property.EffectiveDbBoundness == DbBoundnessType.NotDefined ||
				property.DeclaredDataTypeAttributes.AllDeclaredDatatypeAttributesOnDacProperty.IsDefaultOrEmpty || !property.HasNonNullDataType)
			{
				return;
			}

			var dataTypeAttributesWithDifferentDataTypesOnAggregator = 
				property.DeclaredDataTypeAttributes.DeclaredDataTypeAttributesWithMultipleAggregatedDataTypes;

			if (!dataTypeAttributesWithDifferentDataTypesOnAggregator.IsDefaultOrEmpty)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, dataTypeAttributesWithDifferentDataTypesOnAggregator,
												Descriptors.PX1023_MultipleTypeAttributesOnAggregators);
			}

			if (property.DeclaredDataTypeAttributes.AllDeclaredDatatypeAttributesOnDacProperty.Length > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, 
												property.DeclaredDataTypeAttributes.AllDeclaredDatatypeAttributesOnDacProperty,
												Descriptors.PX1023_MultipleTypeAttributesOnProperty);
			} 
			else if (dataTypeAttributesWithDifferentDataTypesOnAggregator.IsDefaultOrEmpty)
			{		
				CheckDataTypeAttributeAndPropertyTypeForCompatibility(property, pxContext, symbolContext);
			}			
		}

		private static void CheckDataTypeAttributeAndPropertyTypeForCompatibility(DacPropertyInfo property, PXContext pxContext, 
																				 SymbolAnalysisContext symbolContext)
		{
			var dataTypeAttribute = property.DeclaredDataTypeAttributes.AllDeclaredDatatypeAttributesOnDacProperty[0];

			switch (property.EffectivePropertyAndDataTypeAttributeTypesCompatibility)
			{
				case CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.NoDataTypeAttributes:
					ReportIncompatibleTypesDiagnostics(property, dataTypeAttribute, symbolContext, pxContext, registerCodeFix: false);
					return;
				case CompatibilityOfDacPropertyTypeAndTypeFromDataTypeAttributes.IncompatibleTypes:
					ReportIncompatibleTypesDiagnostics(property, dataTypeAttribute, symbolContext, pxContext, registerCodeFix: true);
					return;
			}
		}

		private static void ReportIncompatibleTypesDiagnostics(DacPropertyInfo property, DacFieldAttributeInfo fieldAttribute,
															   SymbolAnalysisContext symbolContext, PXContext pxContext, bool registerCodeFix)
		{
			var diagnosticProperties = ImmutableDictionary.Create<string, string?>()
														  .Add(DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString());

			// Node not null here because aggregated DAC analysers by default run only on DACs in source 
			// and these properties are declared in the DAC type itself
			Location? propertyTypeLocation = property.Node!.Type.GetLocation();
			Location? attributeLocation = fieldAttribute.AttributeData.GetLocation(symbolContext.CancellationToken);

			if (propertyTypeLocation != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, propertyTypeLocation, attributeLocation.ToEnumerable(),
									  diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}

			if (attributeLocation != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation, propertyTypeLocation.ToEnumerable(),
									  diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}
		}

		private static void RegisterDiagnosticForAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
															IEnumerable<DacFieldAttributeInfo> attributesToReport, DiagnosticDescriptor diagnosticDescriptor) =>
			RegisterDiagnosticForAttributes(symbolContext, pxContext,
											attributesToReport.Select(a => a.AttributeData.GetLocation(symbolContext.CancellationToken)
																						  .NullIfLocationKindIsNone()),
											diagnosticDescriptor);

		// Second oveload to prevent the boxing of immutable array
		private static void RegisterDiagnosticForAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
															ImmutableArray<DacFieldAttributeInfo> attributesToReport, DiagnosticDescriptor diagnosticDescriptor) =>
			RegisterDiagnosticForAttributes(symbolContext, pxContext, 
											attributesToReport.Select(a => a.AttributeData.GetLocation(symbolContext.CancellationToken)
																						  .NullIfLocationKindIsNone()), 
											diagnosticDescriptor);
		

		private static void RegisterDiagnosticForAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
															IEnumerable<Location?> attributeLocations, DiagnosticDescriptor diagnosticDescriptor)
		{
			IEnumerable<Location> attributeLocationsToReport = attributeLocations.Where(location => location != null)!;

			foreach (Location location in attributeLocationsToReport)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(diagnosticDescriptor, location),
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}