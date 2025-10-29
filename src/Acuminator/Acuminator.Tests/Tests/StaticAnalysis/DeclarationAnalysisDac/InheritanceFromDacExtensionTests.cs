using Acuminator.Analyzers.StaticAnalysis;
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
    public class InheritanceFromDacExtensionTests : CodeFixVerifier
    {
	    protected override CodeFixProvider GetCSharpCodeFixProvider() => new InheritanceFromDacExtensionFix();
	    
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzer());

		[Theory]
        [EmbeddedFileData("InheritanceFromPXCacheExtension_Good.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

		[Theory]
		[EmbeddedFileData("InheritanceFromPXCacheExtension_Bad.cs")]
		public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual,
	            Descriptors.PX1009_InheritanceFromDacExtension.CreateFor(10, 15),
	            Descriptors.PX1011_NotSealedDacExtension.CreateFor(12, 15),
	            Descriptors.PX1011_NotSealedDacExtension.CreateFor(13, 15));
        }

	    [Theory]
	    [EmbeddedFileData("InheritanceFromPXMappedCacheExtension.cs")]
	    public void TestDiagnostic_PXMappedCacheExtension_ShouldNotShowDiagnostic(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
	    [EmbeddedFileData("InheritanceFromPXCacheExtension_Bad.cs", "InheritanceFromPXCacheExtension_Bad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }
    }
}
