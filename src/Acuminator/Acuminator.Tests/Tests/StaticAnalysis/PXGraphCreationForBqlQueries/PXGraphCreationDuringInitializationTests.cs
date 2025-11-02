using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationForBqlQueries
{
	public class PXGraphCreationForBqlQueriesTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphCreationForBqlQueriesAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithRecursiveAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXGraphCreationForBqlQueriesFix();

		#region Positive checks

		[Theory]
		[EmbeddedFileData("ExternalServiceWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalServiceWithPXGraphConstructor(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

		[Theory]
		[EmbeddedFileData("ExternalServiceWithCreateInstance.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalServiceWithCreateInstance(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

		[Theory]
		[EmbeddedFileData("InstanceMethodInPXGraphWithPXGraphConstructor.cs")]
		public Task InstanceMethodInPXGraphWithPXGraphConstructor(string source) => VerifyCSharpDiagnosticAsync(source,
			Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

		[Theory]
		[EmbeddedFileData("InstanceMethodInPXGraphWithCreateInstance.cs")]
		public Task InstanceMethodInPXGraphWithCreateInstance(string source) => VerifyCSharpDiagnosticAsync(source,
			Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

		[Theory]
		[EmbeddedFileData("PXGraphConstructorInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task PXGraphConstructorInVariable(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(18, 13));

		[Theory]
		[EmbeddedFileData("ExternalServiceWithCreateInstanceInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalServiceWithCreateInstanceInVariable(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(18, 13));

		[Theory]
		[EmbeddedFileData("ExternalServiceWithMethodParameter.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalServiceWithMethodParameter(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

		[Theory]
		[EmbeddedFileData("PropertyInExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task PropertyInExternalService(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(19, 14));

		[Theory]
		[EmbeddedFileData("InstanceMethodInPXGraphExtensionWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task InstanceMethodInPXGraphExtensionWithPXGraphConstructor(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

		[Theory]
		[EmbeddedFileData("CustomerMaint_CheckGraphContext.cs")]
		public Task InsideGraph_OnlyInstanceMethodsAreReported_WithThisReferenceSuggestion(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 50));

		[Theory]
		[EmbeddedFileData("ExternalService_TwoQueryWithGraphCreation.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task TwoQuery_WithGraphCreationInArgument(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable.CreateFor(14, 51),
				Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable.CreateFor(15, 51));
		#endregion

		#region False-positive checks

		[Theory]
		[EmbeddedFileData("ExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalService_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		[Theory]
		[EmbeddedFileData("StaticMethodInPXGraphWithPXGraphConstructor.cs")]
		public Task StaticMethodInPXGraphWithPXGraphConstructor_ShouldNotShowDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ExternalService_SingleQueryWithGraphCreation.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task SingleQuery_WithGraphCreationInArgument_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		[Theory]
		[EmbeddedFileData("ExternalService_TwoQueriesReuseGraphField.cs", "Customer.cs")]
		public Task TwoQueries_ReusingSameGraph_InArguments_ShouldNotShowDiagnostic(string source, string dacSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource);

		[Theory]
		[EmbeddedFileData("ExternalService_TwoQueries_UseDifferentGraphFields.cs", "Customer.cs")]
		public Task TwoQueries_UseDifferentGraphFields_InArguments_ShouldNotShowDiagnostic(string source, string dacSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource);


		[Theory]
		[EmbeddedFileData("InstanceIsUsedOutsideBql.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task InstanceIsUsedOutsideBql_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		[Theory]
		[EmbeddedFileData("ExternalServiceWithUsedVariable.cs", "Customer.cs", "CustomerMaint.cs")]
		public Task ExternalServiceWithUsedVariable_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
			VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		#endregion

		#region Code Fixes

		[Theory]
		[EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_ThisKeyword.cs")]
		public Task CodeFix_ThisKeyword(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 0);

		[Theory]
		[EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_MethodParameter.cs")]
		public Task CodeFix_MethodParameter(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 1);

		[Theory]
		[EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable1.cs")]
		public Task CodeFix_LocalVariable1(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 2);

		[Theory]
		[EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable2.cs")]
		public Task CodeFix_LocalVariable2(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 3);

		[Theory]
		[EmbeddedFileData("CodeFix_GraphExtension.cs", "CodeFix_GraphExtension_Expected.cs")]
		public Task CodeFix_GraphExtension(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		#endregion
	}
}
