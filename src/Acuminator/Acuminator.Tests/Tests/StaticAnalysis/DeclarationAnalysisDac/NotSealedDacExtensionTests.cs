using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InheritanceFromPXCacheExtension
{
	public class NotSealedDacExtensionTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new InheritanceFromDacExtensionMakeSealedFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("InheritanceFromPXCacheExtensionMakeSealed_Bad.cs", "InheritanceFromPXCacheExtensionMakeSealed_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
