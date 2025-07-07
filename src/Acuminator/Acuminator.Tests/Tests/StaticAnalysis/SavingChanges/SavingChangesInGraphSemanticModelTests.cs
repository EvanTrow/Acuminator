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
		public Task GraphInstance_InitializationMethod_And_Constructor(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(18, 5),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(23, 4),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(28, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphExtensionIsSavingChanges.cs")]
		public Task GraphExtension_SaveCHanges_InitializationMethod_And_Constructor(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(16, 5),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(22, 4),
				Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(28, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphNotSavingChanges.cs")]
		public Task Graph_NotSavingChanges_DoesntReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegate.cs")]
		public Task ViewDelegate(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateWithParameter.cs")]
		public Task ViewDelegateWithParameter(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOwnView.cs")]
		public Task ViewDelegateInGraphExtensionOwnView(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(12, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionBaseView.cs")]
		public Task ViewDelegateInGraphExtensionBaseView(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOverride.cs")]
		public Task ViewDelegateInGraphExtensionOverride(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));
	}
}
