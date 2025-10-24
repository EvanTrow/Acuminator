using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

public class MissingMandatoryDacFieldsAnalyzer : DacAggregatedAnalyzerBase
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create
		(
			Descriptors.PX1069_MissingSingleMandatoryDacField,
			Descriptors.PX1069_MissingMultipleMandatoryDacFields,
			Descriptors.PX1110_MissingNoteIdFieldInDacWithLocalizableFieldValues
		);

	public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) => 
		base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.Dac &&
		!dac.IsFullyUnbound && !dac.HasAccumulatorAttribute && dac.DacFieldsByNames.Count > 0;

	public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
	{
		if (!dac.IsProjectionDac)
		{
			var missingMandatoryDacFieldKinds = GetMissingMandatoryDacFieldsInfos(dac, symbolContext.CancellationToken);

			if (missingMandatoryDacFieldKinds.Count > 0)
			{
				ReportMissingMandatoryTimestampAndAuditDacFields(symbolContext, pxContext, dac, missingMandatoryDacFieldKinds);
			}
		}

		var missingNoteIdFieldInfo = GetMissingNoteIdFieldInfo(dac, pxContext);

		if (missingNoteIdFieldInfo.HasValue)
		{
			ReportMissingNoteIdDacField(symbolContext, pxContext, dac, missingNoteIdFieldInfo.Value);
		}
	}

	private static List<MissingMandatoryDacFieldInfo> GetMissingMandatoryDacFieldsInfos(DacSemanticModel dac, CancellationToken cancellation)
	{
		var missingMandatoryDacFieldCategories = GetMandatoryDacFieldCategories();

		// Check every DAC field in this DAC and its base DACs if there are any to see that if all mandatory DAC fields are present
		foreach (var dacField in dac.DacFields)
		{
			cancellation.ThrowIfCancellationRequested();

			if (dacField.FieldCategory == DacFieldCategory.tstamp || dacField.FieldCategory.IsAuditField())
				missingMandatoryDacFieldCategories.Remove(dacField.FieldCategory);
		}

		// cheap check for presence of declared DAC fields
		if (dac.Node?.Members.Count is null or 0)
		{
			var missingMandatoryFields =
				missingMandatoryDacFieldCategories.Select(fieldCategory => new MissingMandatoryDacFieldInfo(fieldCategory, DacFieldInsertMode.AtTheEnd))
												  .ToList(missingMandatoryDacFieldCategories.Count);
			return missingMandatoryFields;
		}

		var (hasCreatedAuditFields, hasLastModifiedAuditFields) = CheckForDeclaredAuditFields(dac);
		var missingMandatoryDacFieldInfos = new List<MissingMandatoryDacFieldInfo>(missingMandatoryDacFieldCategories.Count);

		foreach (DacFieldCategory missingFieldCategory in missingMandatoryDacFieldCategories)
		{
			var insertMode = GetInsertModeForFieldCategory(missingFieldCategory, hasCreatedAuditFields, hasLastModifiedAuditFields);

			if (insertMode != null)
			{
				missingMandatoryDacFieldInfos.Add(new MissingMandatoryDacFieldInfo(missingFieldCategory, insertMode.Value)); 
			}
		}

		return missingMandatoryDacFieldInfos;
	}

	private MissingMandatoryDacFieldInfo? GetMissingNoteIdFieldInfo(DacSemanticModel dac, PXContext pxContext)
	{
		bool hasNoteID = dac.PropertiesByNames.ContainsKey(DacFieldNames.System.NoteID);
		var pxDBLocalizableStringAttribute = pxContext.FieldAttributes.PXDBLocalizableStringAttribute;
		MissingMandatoryDacFieldInfo? missingNoteIdFieldInfo = null;

		if (!hasNoteID && pxDBLocalizableStringAttribute != null)
		{
			bool hasFieldWithLocalizableValues =
				 dac.DacFieldPropertiesWithAcumaticaAttributes
					.Where(p => p.PropertyTypeUnwrappedNullable.SpecialType == SpecialType.System_String)
					.SelectMany(property => property.DeclaredDataTypeAttributes.AllDeclaredDatatypeAttributesOnDacProperty)
					.Any(attributeInfo => attributeInfo.AggregatesAttribute(pxDBLocalizableStringAttribute));

			if (hasFieldWithLocalizableValues)
			{
				missingNoteIdFieldInfo = new MissingMandatoryDacFieldInfo(DacFieldCategory.NoteID, DacFieldInsertMode.AtTheEnd);
			}
		}

		return missingNoteIdFieldInfo;
	}

	private static List<DacFieldCategory> GetMandatoryDacFieldCategories() =>
		[
			DacFieldCategory.tstamp,
			DacFieldCategory.CreatedByID,
			DacFieldCategory.CreatedByScreenID,
			DacFieldCategory.CreatedDateTime,
			DacFieldCategory.LastModifiedByID,
			DacFieldCategory.LastModifiedByScreenID,
			DacFieldCategory.LastModifiedDateTime
		];

	private static (bool HasCreatedAuditFields, bool HasLastModifiedAuditFields) CheckForDeclaredAuditFields(DacSemanticModel dac)
	{
		bool hasCreatedAuditFields = false;
		bool hasLastModifiedAuditFields = false;

		foreach (var dacField in dac.DeclaredDacFields)
		{
			if (dacField.FieldCategory.IsCreatedAuditField())
				hasCreatedAuditFields = true;
			else if (dacField.FieldCategory.IsLastModifiedAuditField())
				hasLastModifiedAuditFields = true;

			if (hasCreatedAuditFields && hasLastModifiedAuditFields)
				break;
		}

		return (hasCreatedAuditFields, hasLastModifiedAuditFields);
	}

	private static DacFieldInsertMode? GetInsertModeForFieldCategory(DacFieldCategory missingFieldCategory, bool hasCreatedAuditFields, 
																	 bool hasLastModifiedAuditFields) =>
		missingFieldCategory switch
		{
			DacFieldCategory.CreatedByID or
			DacFieldCategory.CreatedByScreenID or
			DacFieldCategory.CreatedDateTime 	  		=> hasCreatedAuditFields
															? (missingFieldCategory == DacFieldCategory.CreatedByID
																? DacFieldInsertMode.BeforeFirstCreatedAuditField
																: DacFieldInsertMode.AfterLastCreatedAuditField)
															: (hasLastModifiedAuditFields
																? DacFieldInsertMode.BeforeFirstLastModifiedAuditField
																: DacFieldInsertMode.AtTheEnd),
			DacFieldCategory.LastModifiedByID or
			DacFieldCategory.LastModifiedByScreenID or
			DacFieldCategory.LastModifiedDateTime		=> hasLastModifiedAuditFields
															? (missingFieldCategory == DacFieldCategory.LastModifiedByID
																? DacFieldInsertMode.BeforeFirstLastModifiedAuditField
																: DacFieldInsertMode.AfterLastLastModifiedAuditField)
															: (hasCreatedAuditFields 
																? DacFieldInsertMode.AfterLastCreatedAuditField
																: DacFieldInsertMode.AtTheEnd),
			DacFieldCategory.tstamp 			  		=> DacFieldInsertMode.AtTheEnd,
			_									  		=> null
		};

	protected virtual void ReportMissingMandatoryTimestampAndAuditDacFields(SymbolAnalysisContext symbolContext, PXContext pxContext, 
														DacSemanticModel dac, List<MissingMandatoryDacFieldInfo> missingMandatoryDacFieldInfos)
	{
		symbolContext.CancellationToken.ThrowIfCancellationRequested();

		var location = dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ?? 
					   dac.Symbol.Locations.FirstOrDefault();
		Diagnostic diagnostic;

		if (missingMandatoryDacFieldInfos.Count == 1)
		{
			var (missingDacField, insertMode) = missingMandatoryDacFieldInfos[0];
			var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperties.MissingMandatoryDacFieldsInfos, $"{missingDacField}{Constants.FieldCategoryAndInsertModeSeparator}{insertMode}" },
				{ DiagnosticProperties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			string[] messageArgs = [dac.Name, $"\"{missingDacField}\"" ];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingSingleMandatoryDacField, location, properties, messageArgs);
		}
		else
		{
			var missingDacFieldsInfos = 
				missingMandatoryDacFieldInfos.Select(info => $"{info.FieldCategory}{Constants.FieldCategoryAndInsertModeSeparator}{info.InsertMode}")
											 .ToList(missingMandatoryDacFieldInfos.Count);
			var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperties.MissingMandatoryDacFieldsInfos, missingDacFieldsInfos.Join(Constants.FieldCategoriesSeparator) },
				{ DiagnosticProperties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			var missingFieldsFormatArg = missingMandatoryDacFieldInfos.Select(info => $"\"{info.FieldCategory}\"")
																	  .Join(", ");
			string[] messageArgs = [dac.Name, missingFieldsFormatArg];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingMultipleMandatoryDacFields, location, properties, messageArgs);
		}

		symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
	}

	protected virtual void ReportMissingNoteIdDacField(SymbolAnalysisContext symbolContext, PXContext pxContext,
													   DacSemanticModel dac, MissingMandatoryDacFieldInfo missingNoteIdFieldInfo)
	{
		symbolContext.CancellationToken.ThrowIfCancellationRequested();

		var location = dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ??
					   dac.Symbol.Locations.FirstOrDefault();

		var (missingDacField, insertMode) = missingNoteIdFieldInfo;
		var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperties.MissingMandatoryDacFieldsInfos, $"{missingDacField}{Constants.FieldCategoryAndInsertModeSeparator}{insertMode}" },
				{ DiagnosticProperties.IsSealedDac,						dac.Symbol.IsSealed.ToString() }
			}
		.ToImmutableDictionary();

		var diagnostic = Diagnostic.Create(Descriptors.PX1110_MissingNoteIdFieldInDacWithLocalizableFieldValues, location, properties, dac.Name);
		symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
	}
}