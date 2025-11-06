using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
	public class ActionDelegatesWithLongRunReturnTypeTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
					CodeAnalysisSettings.Default
										.WithStaticAnalysisEnabled()
										.WithRecursiveAnalysisEnabled()
										.WithSuppressionMechanismDisabled(),
					new LongOperationInGraphAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new ActionHandlerReturnTypeFix();

		[Theory]
		[EmbeddedFileData(@"ActionHandlerReturnType\GraphWithActionDelegatesWithBadSignatures.cs")]
		public Task ActionDelegates_StartingLongRuns_WithIncorrectSignatures(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(22, 15),
				Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(36, 15),
				Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(41, 15),
				Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(48, 15),
				Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(55, 15));

		[Theory]
		[EmbeddedFileData(@"ActionHandlerReturnType\GraphWithActionDelegatesWithBadSignatures_Expected.cs")]
		public Task ActionDelegates_StartingLongRuns_WithCorrectSignatures_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"ActionHandlerReturnType\GraphWithActionDelegatesWithBadSignatures.cs",
						  @"ActionHandlerReturnType\GraphWithActionDelegatesWithBadSignatures_Expected.cs")]
		public Task ActionDelegates_StartingLongRuns_WithIncorrectSignatures_CodeFix_FixSignatures(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}
