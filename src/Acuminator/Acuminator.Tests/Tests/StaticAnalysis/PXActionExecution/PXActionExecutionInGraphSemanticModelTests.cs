using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class PXActionExecutionInGraphSemanticModelTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new PXActionExecutionInGraphSemanticModelAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInGraph.cs")]
		public Task Press_PXGraphInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(12, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(16, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(21, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInGraphExtension.cs")]
		public Task Press_PXGraphExtensionInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(12, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(16, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(21, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraph.cs")]
		public Task PressOnDerivedType_PXGraphInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(14, 31),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(18, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraphExtension.cs")]
		public Task PressOnDerivedType_PXGraphExtensionInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(10, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(14, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(18, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressWithAdapterInGraph.cs")]
		public Task PressWithAdapter_PXGraphInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(12, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(16, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(21, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressWithAdapterInGraphExtension.cs")]
		public Task PressWithAdapter_PXGraphExtensionInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(10, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(14, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(19, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraph.cs")]
		public Task PressWithExternalMethod_PXGraphInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(13, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(17, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(22, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraphExtension.cs")]
		public Task PressWithExternalMethod_PXGraphExtensionInitialization(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(10, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(15, 4),
				Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(21, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInViewDelegate.cs")]
		public Task Press_ViewDelegate(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1082_ActionExecutionInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInViewDelegateWithParameter.cs")]
		public Task Press_ViewDelegateWithParameter(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1082_ActionExecutionInDataViewDelegate.CreateFor(13, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInViewDelegateInGraphExtension.cs")]
		public Task Press_ViewDelegateInGraphExtensionOwnView(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1082_ActionExecutionInDataViewDelegate.CreateFor(12, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInViewDelegateInGraphExtension2.cs")]
		public Task Press_ViewDelegateInGraphExtensionBaseView(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1082_ActionExecutionInDataViewDelegate.CreateFor(10, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\PressInViewDelegateInGraphExtension3.cs")]
		public Task Press_ViewDelegateInGraphExtensionOverride(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1082_ActionExecutionInDataViewDelegate.CreateFor(10, 13));
	}
}
