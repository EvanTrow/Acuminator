using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity
{
	public class DacWrongPrimaryKeyNameTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPrimaryAndUniqueKeyDeclarationAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new IncorrectDeclarationOfDacKeyFix();

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName.cs")]
		public Task Dac_WrongPrimaryKeyName(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1036_WrongDacPrimaryKeyName.CreateFor((9, 16), extraLocation: (6, 2)));

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName_Expected.cs")]
		public Task Dac_WrongPrimaryKeyName_AfterCodeFix_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Dac_WrongPrimaryKeyName.cs", "Dac_WrongPrimaryKeyName_Expected.cs")]
		public Task ChangePrimaryKeyName_VerifyCodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}
