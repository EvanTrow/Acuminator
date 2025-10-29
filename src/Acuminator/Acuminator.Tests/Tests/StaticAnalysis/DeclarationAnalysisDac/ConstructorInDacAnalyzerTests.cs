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

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisDac
{
	public class ConstructorInDacAnalyzerTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzerForPX1028Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstructorInDacFix();

		[Theory]
		[EmbeddedFileData(@"ConstructorInDac\DacWithConstructor.cs")]
		public Task Dac_And_DacExtension_WithConstructors(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 13, column: 10),
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 17, column: 10),
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 74, column: 10),
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 88, column: 10),
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 92, column: 10));

		[Theory]
		[EmbeddedFileData(@"ConstructorInDac\DacWithConstructor_Expected.cs")]
		public Task Dac_And_DacExtension_WithConstructors_AfterCodeFix_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"ConstructorInDac\DacWithConstructor.cs",
						  @"ConstructorInDac\DacWithConstructor_Expected.cs")]
		public Task CodeFixDacWithConstructor(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		private sealed class DacAndDacExtensionDeclarationAnalyzerForPX1028Tests : DacAndDacExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1028_ConstructorInDacDeclaration
				);

			protected override void CheckIfDacExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }

			protected override void ReportDacExtensionInheritance(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }
		}
	}
}
