using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using System.Threading.Tasks;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges
{
	public class SavingChangesInGraphSemanticModelTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new SavingChangesInGraphSemanticModelAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphIsSavingChanges.cs")]
		public async Task GraphInstance_InitializationMethod_And_Constructor(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(18, 5),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(23, 4),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(28, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphExtensionIsSavingChanges.cs")]
		public async Task GraphExtension_SaveCHanges_InitializationMethod_And_Constructor(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(16, 5),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(22, 4),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(28, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphNotSavingChanges.cs")]
		public async Task Graph_NotSavingChanges_DoesntReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegate.cs")]
		public async Task ViewDelegate(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateWithParameter.cs")]
		public async Task ViewDelegateWithParameter(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOwnView.cs")]
		public async Task ViewDelegateInGraphExtensionOwnView(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(12, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionBaseView.cs")]
		public async Task ViewDelegateInGraphExtensionBaseView(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOverride.cs")]
		public async Task ViewDelegateInGraphExtensionOverride(string source) =>
			await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));
	}
}
