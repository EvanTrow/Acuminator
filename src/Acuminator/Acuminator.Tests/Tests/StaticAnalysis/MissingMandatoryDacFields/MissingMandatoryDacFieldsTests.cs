#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

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
		[EmbeddedFileData("SealedDacWithoutAnyMandatoryField_Expected.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_AfterCodeFix_NoDiagnostic(string source) =>
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

		[Theory]
		[EmbeddedFileData("SealedDacWithoutAnyMandatoryField.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_ShouldReport_AllMissingFields(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingMultipleMandatoryDacFields
							.CreateFor(8, 22, "SealedDacWithoutAnyMandatoryField",
									  CreateMissingFieldsFormatArg(
									  [
											DacFieldKind.tstamp,
											DacFieldKind.CreatedByID,
											DacFieldKind.CreatedByScreenID,
											DacFieldKind.CreatedDateTime,
											DacFieldKind.LastModifiedByID,
											DacFieldKind.LastModifiedByScreenID,
											DacFieldKind.LastModifiedDateTime
									  ])));

		[Theory]
		[EmbeddedFileData("DacMissingOnlyTimestampField.cs")]
		public async Task Dac_MissingOnlyTimestampField_ShouldReport_OnlyMissingTimestamp(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingSingleMandatoryDacField
							.CreateFor(7, 15, "DacMissingOnlyTimestampField", "\"tstamp\""));

		[Theory]
		[EmbeddedFileData("DacMissingMultipleAuditFields.cs")]
		public async Task DacMissingMultipleAuditFields_ShouldReport_MultipleMissingFields(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingMultipleMandatoryDacFields
							.CreateFor(10, 15, "DacMissingMultipleAuditFields",
									  CreateMissingFieldsFormatArg(
									  [
											DacFieldKind.CreatedByScreenID,
											DacFieldKind.CreatedDateTime,
											DacFieldKind.LastModifiedByScreenID,
											DacFieldKind.LastModifiedDateTime
									  ])));

		[Theory]
		[EmbeddedFileData("DacMissingOnlyCreatedByID.cs")]
		public async Task DacMissingOnlyCreatedByID_ShouldReport_OnlyMissingCreatedByID(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingSingleMandatoryDacField
							.CreateFor(8, 15, "DacMissingOnlyCreatedByID", "\"CreatedByID\""));

		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData("SealedDacWithoutAnyMandatoryField.cs", "SealedDacWithoutAnyMandatoryField_Expected.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_CodeFix_ShouldAdd_AllMandatoryFields_AsNonVirtual(
																				string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyTimestampField.cs", "DacMissingOnlyTimestampField_Expected.cs")]
		public async Task DacMissingOnlyTimestampField_CodeFix_ShouldAdd_TimestampField(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingOnlyCreatedByID.cs", "DacMissingOnlyCreatedByID_Expected.cs")]
		public async Task DacMissingOnlyCreatedByID_CodeFix_ShouldAdd_CreatedByIDField(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("DacMissingMultipleAuditFields.cs", "DacMissingMultipleAuditFields_Expected.cs")]
		public async Task DacMissingMultipleAuditFields_CodeFix_ShouldAdd_MissingFields(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);
		#endregion


		private static string CreateMissingFieldsFormatArg(IEnumerable<DacFieldKind> dacFieldKinds) =>
			dacFieldKinds.Select(fieldKind => $"\"{fieldKind.ToString()}\"")
						 .Join(", ");
	}
}