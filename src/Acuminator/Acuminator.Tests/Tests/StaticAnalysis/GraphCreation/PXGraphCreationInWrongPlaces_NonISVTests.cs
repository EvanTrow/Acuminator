#nullable enable
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using System.Threading.Tasks;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInWrongPlaces_NonISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithStaticAnalysisEnabled()
					.WithSuppressionMechanismDisabled()
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new PXGraphCreationInGraphInWrongPlacesGraphAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInitialization.cs")]
		public Task GraphInstanceConstructor(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(13, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(19, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(25, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(27, 12));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitialization.cs")]
		public Task GraphExtensionInitialize(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(14, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(19, 32),
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(26, 32));

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
	}
}