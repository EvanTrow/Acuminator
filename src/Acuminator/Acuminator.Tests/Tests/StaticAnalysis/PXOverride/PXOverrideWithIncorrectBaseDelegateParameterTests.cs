#nullable enable
using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideWithIncorrectBaseDelegateParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForIncorrectBaseDelegateParameterTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddOrReplaceBaseDelegateParameterFix();

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegate_Parameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(14, 63),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(18, 9),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(24, 10),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(30, 27));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegate_Parameter_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter.cs",
						  @"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_Without_BaseDelegate_Parameter_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);


		private sealed class PXOverrideAnalyzerForIncorrectBaseDelegateParameterTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter
				);

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																			   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext, 
																		IMethodSymbol patchMethodWithPXOverride)
			{ }
		}
	}
}