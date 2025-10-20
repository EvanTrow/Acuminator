using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
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
			Descriptors.PX1069_MissingMultipleMandatoryDacFields
		);

	public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) => 
		base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.Dac &&
		!dac.IsFullyUnbound && !dac.HasAccumulatorAttribute && dac.DacFieldsByNames.Count > 0;

	public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
	{
		var missingMandatoryDacFieldKinds = GetMissingMandatoryDacFieldsInfos(dac, symbolContext.CancellationToken);

		if (missingMandatoryDacFieldKinds.Count > 0)
		{
			ReportMissingMandatoryDacFields(symbolContext, pxContext, dac, missingMandatoryDacFieldKinds);
		}
	}

	private static List<(DacFieldCategory FieldCategory, DacFieldInsertMode InsertMode)> GetMissingMandatoryDacFieldsInfos(DacSemanticModel dac, 
																									  CancellationToken cancellation)
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
		if (dac.Node == null || dac.Node.Members.Count == 0)
		{
			return missingMandatoryDacFieldCategories.Select(fieldCategory => (fieldCategory, DacFieldInsertMode.AtTheEnd))
													 .ToList(missingMandatoryDacFieldCategories.Count);
		}

		var (hasCreatedAuditFields, hasLastModifiedAuditFields) = CheckForDeclaredAuditFields(dac);

		var missingMandatoryDacFieldInfos = 
			new List<(DacFieldCategory FieldCategory, DacFieldInsertMode InsertMode)>(missingMandatoryDacFieldCategories.Count);

		foreach (DacFieldCategory missingFieldCategory in missingMandatoryDacFieldCategories)
		{
			var insertMode = GetInsertModeForFieldCategory(missingFieldCategory, hasCreatedAuditFields, hasLastModifiedAuditFields);

			if (insertMode != null)
			{
				missingMandatoryDacFieldInfos.Add((FieldCategory: missingFieldCategory, InsertMode: insertMode.Value)); 
			}
		}

		return missingMandatoryDacFieldInfos;
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

	private static void ReportMissingMandatoryDacFields(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
														List<(DacFieldCategory FieldCategory, DacFieldInsertMode InsertMode)> missingMandatoryDacFieldInfos)
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
				{ PX1069Properties.MissingMandatoryDacFieldsInfos, $"{missingDacField}{Constants.FieldCategoryAndInsertModeSeparator}{insertMode}" },
				{ PX1069Properties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
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
				{ PX1069Properties.MissingMandatoryDacFieldsInfos, missingDacFieldsInfos.Join(Constants.FieldCategoriesSeparator) },
				{ PX1069Properties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			var missingFieldsFormatArg = missingMandatoryDacFieldInfos.Select(info => $"\"{info.FieldCategory}\"")
																	  .Join(", ");
			string[] messageArgs = [dac.Name, missingFieldsFormatArg];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingMultipleMandatoryDacFields, location, properties, messageArgs);
		}

		symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
	}
}