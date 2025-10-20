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
		[EmbeddedFileData(@"CodeFix\SealedDacWithoutAnyMandatoryField_Expected.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingTimestampFieldToEnd_Expected.cs")]
		public async Task Dac_MissingTimestampField_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdBetweenDacFields_Expected.cs")]
		public async Task Dac_MissingCreatedByIdField_AfterCodeFix_AddedToEnd_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdToBeginning_Expected.cs")]
		public async Task Dac_MissingCreatedByIdField_AfterCodeFix_AddedToBeginning_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacMissingMultipleAuditFields_Expected.cs")]
		public async Task DacMissingMultipleAuditFields_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion

		[Theory]
		[EmbeddedFileData(@"CodeFix\SealedDacWithoutAnyMandatoryField.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_ShouldReport_AllMissingFields(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingMultipleMandatoryDacFields
							.CreateFor(8, 22, "SealedDacWithoutAnyMandatoryField",
									  CreateMissingFieldsFormatArg(
									  [
											DacFieldCategory.tstamp,
											DacFieldCategory.CreatedByID,
											DacFieldCategory.CreatedByScreenID,
											DacFieldCategory.CreatedDateTime,
											DacFieldCategory.LastModifiedByID,
											DacFieldCategory.LastModifiedByScreenID,
											DacFieldCategory.LastModifiedDateTime
									  ])));

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingTimestampFieldToEnd.cs")]
		public async Task Dac_MissingOnlyTimestampField_ShouldReport_OnlyMissingTimestamp(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingSingleMandatoryDacField
							.CreateFor(8, 15, "DacAddMissingTimestampFieldToEnd", "\"tstamp\""));

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacMissingMultipleAuditFields.cs")]
		public async Task DacMissingMultipleAuditFields_ShouldReport_MultipleMissingFields(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingMultipleMandatoryDacFields
							.CreateFor(9, 15, "DacMissingMultipleAuditFields",
									  CreateMissingFieldsFormatArg(
									  [
											DacFieldCategory.CreatedByScreenID,
											DacFieldCategory.CreatedDateTime,
											DacFieldCategory.LastModifiedByScreenID,
											DacFieldCategory.LastModifiedDateTime
									  ])));

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdBetweenDacFields.cs")]
		public async Task Dac_MissingCreatedByIdField_ShouldReport_OnlyMissingCreatedByID(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingSingleMandatoryDacField
							.CreateFor(8, 15, "DacAddMissingCreatedByIdBetweenDacFields", "\"CreatedByID\""));

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdToBeginning.cs")]
		public async Task Dac_MissingCreatedByIdField_DifferentFieldOrder_ShouldReport_OnlyMissingCreatedByID(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1069_MissingSingleMandatoryDacField
							.CreateFor(10, 15, "DacAddMissingCreatedByIdToBeginning", "\"CreatedByID\""));

		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData(@"CodeFix\SealedDacWithoutAnyMandatoryField.cs", @"CodeFix\SealedDacWithoutAnyMandatoryField_Expected.cs")]
		public async Task SealedDac_WithoutAnyMandatoryField_CodeFix_ShouldAdd_AllMandatoryFields_AsNonVirtual(
																				string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingTimestampFieldToEnd.cs", @"CodeFix\DacAddMissingTimestampFieldToEnd_Expected.cs")]
		public async Task Dac_MissingTimestampField_CodeFix_ShouldAdd_Field_ToEnd(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdBetweenDacFields.cs", @"CodeFix\DacAddMissingCreatedByIdBetweenDacFields_Expected.cs")]
		public async Task Dac_MissingCreatedByIdField_CodeFix_ShouldAdd_Field_BetweenDacFields(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacAddMissingCreatedByIdToBeginning.cs", @"CodeFix\DacAddMissingCreatedByIdToBeginning_Expected.cs")]
		public async Task Dac_MissingCreatedByIdField_CodeFix_ShouldAdd_Field_ToBeginning(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\DacMissingMultipleAuditFields.cs", @"CodeFix\DacMissingMultipleAuditFields_Expected.cs")]
		public async Task DacMissingMultipleAuditFields_CodeFix_ShouldAdd_MissingFields(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);
		#endregion

		private static string CreateMissingFieldsFormatArg(IEnumerable<DacFieldCategory> dacFieldCategories) =>
			dacFieldCategories.Select(fieldCategory => $"\"{fieldCategory.ToString()}\"")
							  .Join(", ");
	}
}