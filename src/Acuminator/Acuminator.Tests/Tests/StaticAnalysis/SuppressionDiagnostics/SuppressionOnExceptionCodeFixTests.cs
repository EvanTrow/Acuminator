using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class SuppressionOnExceptionCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LocalizationPXExceptionAndPXExceptionInfoAnalyzer(
				CodeAnalysisSettings.Default
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismEnabled()
									.WithRecursiveAnalysisEnabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"Exceptions\PXCustomException_Expected.cs")]
		public virtual Task CustomException_Alert_InChainedConstructorCall_Suppressed(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Exceptions\PXCustomExceptionDifferentFormat_Expected.cs")]
		public virtual Task CustomException_DifferentFormat_Alert_InChainedConstructorCall_Suppressed(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Exceptions\PXCustomException.cs", @"Exceptions\PXCustomException_Expected.cs")]
		public virtual Task CustomException_Alert_InChainedConstructorCall_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Exceptions\PXCustomExceptionDifferentFormat.cs", @"Exceptions\PXCustomExceptionDifferentFormat_Expected.cs")]
		public virtual Task CustomException_DifferentFormat_Alert_InChainedConstructorCall_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}