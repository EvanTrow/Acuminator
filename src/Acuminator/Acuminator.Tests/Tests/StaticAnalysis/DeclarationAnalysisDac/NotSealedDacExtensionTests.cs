using System;
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
	public class NotSealedDacExtensionTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzerForPX1011Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MakeSealedDacExtensionFix();

		[Theory]
		[EmbeddedFileData(@"NotSealedDacExtension\NotSealedDacExtension.cs")]
		public Task DacExtension_NotSealed(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1011_NotSealedDacExtension.CreateFor(11, 15));

		[Theory]
		[EmbeddedFileData(@"NotSealedDacExtension\NotSealedDacExtension_Expected.cs")]
		public Task DacExtension_NotSealed_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"NotSealedDacExtension\NotSealedDacExtension.cs", 
						  @"NotSealedDacExtension\NotSealedDacExtension_Expected.cs")]
		public Task DacExtension_NotSealed_CodeFix_MakeSealed(string source, string expected) =>
			VerifyCSharpFixAsync(source, expected);


		private sealed class DacAndDacExtensionDeclarationAnalyzerForPX1011Tests : DacAndDacExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1011_NotSealedDacExtension
				);

			protected override void ReportDacExtensionInheritance(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }
		}
	}
}
