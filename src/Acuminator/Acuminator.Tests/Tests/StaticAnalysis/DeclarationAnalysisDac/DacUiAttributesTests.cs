using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacUiAttributes
{
	public class DacUiAttributesTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzerForPX1094Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacUiAttributesFix();

		[Theory]
		[EmbeddedFileData("Dac_Bad.cs")]
		public Task Dac_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1094_DacShouldHaveUiAttribute.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData("Dac_Good_Hidden.cs")]
		public Task Dac_WithPXHiddenAttribute_DoesntReportDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Dac_Good_CacheName.cs")]
		public Task Dac_WithPXCacheNameAttribute_DoesntReportDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("PXMappedCacheExtension.cs")]
		public Task PXMappedCacheExtension_DoesntReportDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(
			"Dac_Bad.cs",
			"Dac_Good_Hidden.cs")]
		public Task AddPXHiddenAttribute_VerifyCodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 0);

		[Theory]
		[EmbeddedFileData(
			"Dac_Bad.cs",
			"Dac_Good_CacheName.cs")]
		public Task AddPXCacheNameAttribute_VerifyCodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected, 1);


		private sealed class DacAndDacExtensionDeclarationAnalyzerForPX1094Tests : DacAndDacExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1094_DacShouldHaveUiAttribute
				);

			protected override void ReportDacExtensionInheritance(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }

			protected override void CheckForConstructors(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
			{ }

			protected override void CheckIfDacExtensionHasNonTerminalBaseExtensions(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }

			protected override void CheckIfDacExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }
		}
	}
}
