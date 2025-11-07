using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using System.Threading.Tasks;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInLongRunningOperationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithStaticAnalysisEnabled()
									.WithRecursiveAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new ThrowingExceptionsInLongRunningOperationAnalyzer());

		[Theory]
		[EmbeddedFileData(@"LongOperations\LongOperationStart_Bad.cs")]
		public Task LongOperationStart_Throws_SetupNotEnteredException_InLongRun(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(25, 46),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(28, 61),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(34, 56),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(40, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(47, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(53, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(60, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(65, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(70, 4));

		[Theory]
		[EmbeddedFileData(@"LongOperations\ParametersDelegate_Bad.cs")]
		public Task ParametersDelegate_Throws_SetupNotEnteredException_InLongRun(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(39, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(54, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(60, 44),

				// Not reported due to ATR-922
				//Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(63, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(77, 5));

		[Theory]
		[EmbeddedFileData(@"LongOperations\ProcessDelegate_Bad.cs")]
		public Task ProcessDelegate_Throws_SetupNotEnteredException_InLongRun(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(32, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(36, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(38, 48),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(42, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(46, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(48, 58),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(53, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(59, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(63, 89),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(69, 5),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(75, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(80, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(85, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(90, 4));

		[Theory]
		[EmbeddedFileData(@"LongOperations\FinallyProcessDelegate_Bad.cs")]
		public Task FinallyProcessDelegate_Throws_SetupNotEnteredException_InLongRun(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(35, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(38, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(44, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(44, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(48, 6),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(51, 41),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(52, 29),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(55, 63),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(67, 14),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(73, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(78, 4),
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(83, 4));

		[Theory]
		[EmbeddedFileData(@"LongOperations\LongOperationStart_Good.cs")]
		public Task LongOperationStart_WithoutPXSetupNotEnteredException_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"LongOperations\ParametersDelegate_Good.cs")]
		public Task ParametersDelegate_WithoutPXSetupNotEnteredException_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"LongOperations\ProcessDelegate_Good.cs")]
		public Task ProcessDelegate_WithoutPXSetupNotEnteredException_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"LongOperations\FinallyProcessDelegate_Good.cs")]
		public Task FinallyProcessDelegate_WithoutPXSetupNotEnteredException_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);
	}
}
