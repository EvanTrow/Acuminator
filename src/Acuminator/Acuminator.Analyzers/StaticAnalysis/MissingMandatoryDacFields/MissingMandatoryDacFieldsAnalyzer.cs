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

	private static List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)> GetMissingMandatoryDacFieldsInfos(DacSemanticModel dac, 
																									  CancellationToken cancellation)
	{
		var missingMandatoryDacFieldKinds = GetMandatoryDacFieldKinds();

		// Check every DAC field in this DAC and its base DACs if there are any to see that if all mandatory DAC fields are present
		foreach (var dacField in dac.DacFields)
		{
			cancellation.ThrowIfCancellationRequested();

			if (dacField.FieldKind == DacFieldKind.tstamp || dacField.FieldKind.IsAuditField())
				missingMandatoryDacFieldKinds.Remove(dacField.FieldKind);
		}

		// cheap check for presence of declared DAC fields
		if (dac.Node == null || dac.Node.Members.Count == 0)
		{
			return missingMandatoryDacFieldKinds.Select(fieldKind => (FieldKind: fieldKind, InsertMode: DacFieldInsertMode.AtTheEnd))
												.ToList(missingMandatoryDacFieldKinds.Count);
		}

		var (hasCreatedAuditFields, hasLastModifiedAuditFields) = CheckForDeclaredAuditFields(dac);

		var missingMandatoryDacFieldKindsWithIndexes = 
			new List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)>(missingMandatoryDacFieldKinds.Count);

		foreach (DacFieldKind missingFieldKind in missingMandatoryDacFieldKinds)
		{
			var insertMode = GetInsertModeForFieldKind(missingFieldKind, hasCreatedAuditFields, hasLastModifiedAuditFields);

			if (insertMode != null)
			{
				missingMandatoryDacFieldKindsWithIndexes.Add((FieldKind: missingFieldKind, InsertMode: insertMode.Value)); 
			}
		}

		return missingMandatoryDacFieldKindsWithIndexes;
	}

	private static List<DacFieldKind> GetMandatoryDacFieldKinds() =>
		[
			DacFieldKind.tstamp,
			DacFieldKind.CreatedByID,
			DacFieldKind.CreatedByScreenID,
			DacFieldKind.CreatedDateTime,
			DacFieldKind.LastModifiedByID,
			DacFieldKind.LastModifiedByScreenID,
			DacFieldKind.LastModifiedDateTime
		];

	private static (bool HasCreatedAuditFields, bool HasLastModifiedAuditFields) CheckForDeclaredAuditFields(DacSemanticModel dac)
	{
		bool hasCreatedAuditFields = false;
		bool hasLastModifiedAuditFields = false;

		foreach (var dacField in dac.DeclaredDacFields)
		{
			if (dacField.FieldKind.IsCreatedAuditField())
				hasCreatedAuditFields = true;
			else if (dacField.FieldKind.IsLastModifiedAuditField())
				hasLastModifiedAuditFields = true;

			if (hasCreatedAuditFields && hasLastModifiedAuditFields)
				break;
		}

		return (hasCreatedAuditFields, hasLastModifiedAuditFields);
	}

	private static DacFieldInsertMode? GetInsertModeForFieldKind(DacFieldKind missingFieldKind, bool hasCreatedAuditFields, 
																bool hasLastModifiedAuditFields) =>
		missingFieldKind switch
		{
			DacFieldKind.CreatedByID or
			DacFieldKind.CreatedByScreenID or
			DacFieldKind.CreatedDateTime 	  => hasCreatedAuditFields
													? (missingFieldKind == DacFieldKind.CreatedByID
														? DacFieldInsertMode.BeforeFirstCreatedAuditField
														: DacFieldInsertMode.AfterLastCreatedAuditField)
													: (hasLastModifiedAuditFields
														? DacFieldInsertMode.BeforeFirstLastModifiedAuditField
														: DacFieldInsertMode.AtTheEnd),
			DacFieldKind.LastModifiedByID or
			DacFieldKind.LastModifiedByScreenID or
			DacFieldKind.LastModifiedDateTime => hasLastModifiedAuditFields
														? (missingFieldKind == DacFieldKind.LastModifiedByID
															? DacFieldInsertMode.BeforeFirstLastModifiedAuditField
															: DacFieldInsertMode.AfterLastLastModifiedAuditField)
														: (hasCreatedAuditFields 
															? DacFieldInsertMode.AfterLastCreatedAuditField
															: DacFieldInsertMode.AtTheEnd),
			DacFieldKind.tstamp 			  => DacFieldInsertMode.AtTheEnd,
			_								  => null
		};

	private static void ReportMissingMandatoryDacFields(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
														List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)> missingMandatoryDacFieldInfos)
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
				{ PX1069Properties.MissingMandatoryDacFieldsInfos, $"{missingDacField}{Constants.FieldKindAndInsertModeSeparator}{insertMode}" },
				{ PX1069Properties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			string[] messageArgs = [dac.Name, $"\"{missingDacField}\"" ];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingSingleMandatoryDacField, location, properties, messageArgs);
		}
		else
		{
			var missingDacFieldsStrings = 
				missingMandatoryDacFieldInfos.Select(info => $"{info.FieldKind}{Constants.FieldKindAndInsertModeSeparator}{info.InsertMode}")
											 .ToList(missingMandatoryDacFieldInfos.Count);
			var properties = new Dictionary<string, string?>
			{
				{ PX1069Properties.MissingMandatoryDacFieldsInfos, missingDacFieldsStrings.Join(Constants.FieldKindsSeparator) },
				{ PX1069Properties.IsSealedDac,					   dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			var missingFieldsFormatArg = missingDacFieldsStrings.Select(dacField => $"\"{dacField}\"")
																.Join(", ");
			string[] messageArgs = [dac.Name, missingFieldsFormatArg];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingMultipleMandatoryDacFields, location, properties, messageArgs);
		}

		symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
	}
}