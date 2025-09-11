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
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideWithoutXmlCommentTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForMissingXmlCommentTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddXmlDocCommentWithReferenceToBaseMethodFix();

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment.cs")]
		public Task PXOverrides_WithoutXmlDocComment(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(13, 17),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(19, 15),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(25, 27),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(32, 15));

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment_Expected.cs")]
		public Task PXOverrides_WithoutXmlDocComment_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment.cs",
						  @"XmlComment\PXOverrideWithoutXmlComment_Expected.cs")]
		public Task PXOverrides_WithoutXmlDocComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		private sealed class PXOverrideAnalyzerForMissingXmlCommentTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment
				);

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																			   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext,
																		IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext,
																			PXOverrideInfo pxOverrideInfo)
			{ }
		}
	}
}