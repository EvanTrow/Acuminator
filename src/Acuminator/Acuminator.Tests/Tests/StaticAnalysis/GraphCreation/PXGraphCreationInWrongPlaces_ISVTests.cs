#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInWrongPlaces_ISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithStaticAnalysisEnabled()
					.WithSuppressionMechanismDisabled()
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new PXGraphCreationInGraphInWrongPlacesGraphAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInIsActiveMethods.cs")]
		public Task GraphExtension_IsActiveAndIsActiveForGraph(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod.CreateFor(15, 41),
				Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod.CreateFor(19, 4),
				Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod.CreateFor(20, 4),
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod.CreateFor(27, 11),
				Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod.CreateFor(33, 20),
				Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod.CreateFor(34, 8));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInitialization.cs")]
		public Task GraphInstanceConstructor(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(13, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(19, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(25, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(27, 12));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitialization.cs")]
		public Task GraphExtensionInitialize(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(14, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(19, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(26, 32));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceOutsideOfInitialization.cs")]
		public Task PXGraph_OutsideOfInitialization(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ViewDelegate.cs")]
		public Task ViewDelegate(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(13, 25));

		[Theory]
		[EmbeddedFileData("ViewDelegateWithParameter.cs")]
		public Task ViewDelegateWithParameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(13, 30));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionOwnView.cs")]
		public Task ViewDelegateInGraphExtensionOwnView(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(12, 33));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionBaseView.cs")]
		public Task ViewDelegateInGraphExtensionBaseView(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(10, 25));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionOverride.cs")]
		public Task ViewDelegateInGraphExtensionOverride(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(10, 33));
	}
}