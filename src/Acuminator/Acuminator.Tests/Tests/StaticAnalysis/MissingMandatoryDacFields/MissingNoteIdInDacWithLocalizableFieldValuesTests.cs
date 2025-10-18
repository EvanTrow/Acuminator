using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingMandatoryDacFields
{
	public class MissingNoteIdInDacWithLocalizableFieldValuesTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingNoteIdAnalyzerForPX1110Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new MissingMandatoryDacFieldsFix();

		#region No Diagnostic Scenarios
		[Theory]
		[EmbeddedFileData("DacWithAllMandatoryFields.cs")]
		public async Task DacWithAllMandatoryFields_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithInheritedMandatoryFields.cs", "DacWithAllMandatoryFields.cs")]
		public async Task DacWithInheritedMandatoryFields_NoDiagnostic(string source, string baseDacSource) =>
			await VerifyCSharpDiagnosticAsync(source, baseDacSource);

		[Theory]
		[EmbeddedFileData("DacExtension.cs")]
		public async Task DacExtension_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("UnboundDac.cs")]
		public async Task UnboundDac_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("AccumulatorDac.cs")]
		public async Task AccumulatorDac_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("EmptyDac.cs")]
		public async Task EmptyDac_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"PX1110\DacWithBothNoteIdAndLocalizableField.cs")]
		public async Task Dac_WithBoth_NoteId_And_LocalizableField_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"PX1110\DacWithoutBothNoteIdAndLocalizableField.cs")]
		public async Task Dac_WithoutBoth_NoteId_And_LocalizableField_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion

		#region Code Fix Tests

		#endregion

		private sealed class MissingNoteIdAnalyzerForPX1110Tests : MissingMandatoryDacFieldsAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1110_MissingNoteIdFieldInDacWithLocalizableFieldValues
				);

			protected override void ReportMissingMandatoryTimestampAndAuditDacFields(SymbolAnalysisContext symbolContext, PXContext pxContext,
															DacSemanticModel dac, List<MissingMandatoryDacFieldInfo> missingMandatoryDacFieldInfos)
			{ }
		}
	}
}