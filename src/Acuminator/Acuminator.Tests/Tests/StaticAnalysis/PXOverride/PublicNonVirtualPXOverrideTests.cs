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
	public class PublicNonVirtualPXOverrideTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForPublicNonVirtualPXOverrideTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonPublicOrVirtualPXOverrideFix();

		[Theory]
		[EmbeddedFileData(@"PublicNonVirtual\ExtensionWithNonPublicVirtualPXOverride.cs")]
		public Task NonPublic_And_Virtual_PXOverrides(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(11, 25, "TestMethod1"),
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(17, 27, "TestMethod2"),
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(23, 35, "TestMethod3"),
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(29, 8, "TestMethod4"));

		[Theory]
		[EmbeddedFileData(@"PublicNonVirtual\ExtensionWithVirtualPXOverrideThatCannotBeFixed.cs")]
		public Task Abstract_And_Overriden_PXOverrides(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(11, 24, "TestMethod1"),
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.CreateFor(16, 24, "TestMethod2"));

		[Theory]
		[EmbeddedFileData(@"PublicNonVirtual\ExtensionWithNonPublicVirtualPXOverride_Expected.cs")]
		public Task NonPublic_And_Virtual_PXOverrides_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"PublicNonVirtual\ExtensionWithNonPublicVirtualPXOverride.cs", 
						  @"PublicNonVirtual\ExtensionWithNonPublicVirtualPXOverride_Expected.cs")]
		public Task NonPublic_And_Virtual_PXOverrides_CodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"PublicNonVirtual\ExtensionWithVirtualPXOverrideThatCannotBeFixed.cs",
						  @"PublicNonVirtual\ExtensionWithVirtualPXOverrideThatCannotBeFixed.cs")]
		public Task Abstract_And_Overriden_PXOverrides_Cannot_Apply_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);


		private sealed class PXOverrideAnalyzerForPublicNonVirtualPXOverrideTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create(Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual);

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext, 
																			   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext, 
																		  PXOverrideInfo pxOverrideInfo) 
			{ }
		}
	}
}