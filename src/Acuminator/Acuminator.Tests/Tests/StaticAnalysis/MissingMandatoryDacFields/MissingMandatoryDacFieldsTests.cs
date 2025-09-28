#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingMandatoryDacFields
{
	public class MissingMandatoryDacFieldsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingMandatoryDacFieldsAnalyzer());

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
		[EmbeddedFileData("DacWithoutAnyMandatoryFields_Expected.cs")]
		public async Task DacWithoutAnyMandatoryFields_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyTimestampField_Expected.cs")]
		public async Task DacMissingOnlyTimestampField_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyCreatedByID_Expected.cs")]
		public async Task DacMissingOnlyCreatedByID_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacMissingMultipleAuditFields_Expected.cs")]
		public async Task DacMissingMultipleAuditFields_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion
		
		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData("DacWithoutAnyMandatoryFields.cs", "DacWithoutAnyMandatoryFields_Expected.cs")]
		public async Task DacWithoutAnyMandatoryFields_CodeFix_ShouldAddAllMandatoryFields(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyTimestampField.cs", "DacMissingOnlyTimestampField_Expected.cs")]
		public async Task DacMissingOnlyTimestampField_CodeFix_ShouldAddTimestampField(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyCreatedByID.cs", "DacMissingOnlyCreatedByID_Expected.cs")]
		public async Task DacMissingOnlyCreatedByID_CodeFix_ShouldCreatedByIDField(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingMultipleAuditFields.cs", "DacMissingMultipleAuditFields_Expected.cs")]
		public async Task DacMissingMultipleAuditFields_CodeFix_ShouldAddMissingFields(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);
		#endregion
	}
}