#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideWithoutBaseDelegateParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddOrReplaceBaseDelegateParameterFix();

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter.cs")]
		public Task PXOverrides_Without_BaseDelegate_Parameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(12, 16),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(15, 15),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(20, 17),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(26, 15));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithCustomBaseDelegateParameter.cs")]
		public Task PXOverrides_With_BaseDelegateParameter_WithCustomDelegateTypes(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_Without_BaseDelegate_Parameter_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter.cs",
						  @"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_Without_BaseDelegate_Parameter_CodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);
	}
}