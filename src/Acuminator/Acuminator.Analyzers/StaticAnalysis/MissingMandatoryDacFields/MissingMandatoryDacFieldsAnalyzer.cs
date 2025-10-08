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
		var missingMandatoryDacFieldKinds = GetMissingMandatoryDacFieldKinds(dac, symbolContext.CancellationToken);

		if (missingMandatoryDacFieldKinds.Count > 0)
		{
			ReportMissingMandatoryDacFields(symbolContext, pxContext, dac, missingMandatoryDacFieldKinds);
		}
	}

	private static List<DacFieldKind> GetMissingMandatoryDacFieldKinds(DacSemanticModel dac, CancellationToken cancellation)
	{
		var missingMandatoryDacFieldKinds = GetMandatoryDacFieldKinds();

		// Check every DAC field in this DAC and its base DACs if there are any to see that if all mandatory DAC fields are present
		foreach (var dacField in dac.DacFields)
		{
			cancellation.ThrowIfCancellationRequested();

			if (dacField.FieldKind == DacFieldKind.tstamp || dacField.FieldKind.IsAuditField())
				missingMandatoryDacFieldKinds.Remove(dacField.FieldKind);
		}

		return missingMandatoryDacFieldKinds;
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

	private static void ReportMissingMandatoryDacFields(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac, 
														List<DacFieldKind> missingMandatoryDacFieldKinds)
	{
		symbolContext.CancellationToken.ThrowIfCancellationRequested();

		var location = dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ?? 
					   dac.Symbol.Locations.FirstOrDefault();
		Diagnostic diagnostic;

		if (missingMandatoryDacFieldKinds.Count == 1)
		{
			string missingDacField = missingMandatoryDacFieldKinds[0].ToString();
			var properties = new Dictionary<string, string?>
			{
				{ PX1069Properties.MissingMandatoryDacFields, missingDacField  },
				{ PX1069Properties.IsSealedDac,				  dac.Symbol.IsSealed.ToString() }
			}
			.ToImmutableDictionary();

			string[] messageArgs = [dac.Name, $"\"{missingDacField}\"" ];
			diagnostic = Diagnostic.Create(Descriptors.PX1069_MissingSingleMandatoryDacField, location, properties, messageArgs);
		}
		else
		{
			var missingDacFieldsStrings = missingMandatoryDacFieldKinds.Select(kind => kind.ToString())
																	   .ToList(missingMandatoryDacFieldKinds.Count);
			var properties = new Dictionary<string, string?>
			{
				{ PX1069Properties.MissingMandatoryDacFields, missingDacFieldsStrings.Join(",") },
				{ PX1069Properties.IsSealedDac,				  dac.Symbol.IsSealed.ToString() }
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