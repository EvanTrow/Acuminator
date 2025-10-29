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
	public class InheritanceFromDacExtensionTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzerForInheritanceTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new InheritanceFromDacExtensionFix();
		
		[Theory]
		[EmbeddedFileData(@"InheritanceFromDacExtension\InheritanceFromDacExtension_Good.cs")]
		public Task Sealed_DacExtension_WithCorrectBaseType_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"InheritanceFromDacExtension\InheritanceFromDacExtension.cs")]
		public Task DacExtension_NotSealed_And_DerivedFromAnotherDacExtension(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1009_InheritanceFromDacExtension.CreateFor(10, 15),
				Descriptors.PX1011_NotSealedDacExtension.CreateFor(12, 15),
				Descriptors.PX1011_NotSealedDacExtension.CreateFor(13, 15));

		[Theory]
		[EmbeddedFileData(@"InheritanceFromDacExtension\InheritanceFromMappedDacExtension.cs")]
		public Task MappedDacExtension_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"InheritanceFromDacExtension\InheritanceFromDacExtension.cs",
						  @"InheritanceFromDacExtension\InheritanceFromDacExtension_Expected.cs")]
		public Task DacExtension_DerivedFromAnotherDacExtension_CodeFix_ChangeBaseType(string source, string expected) =>
			VerifyCSharpFixAsync(source, expected);


		private sealed class DacAndDacExtensionDeclarationAnalyzerForInheritanceTests : DacAndDacExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1009_InheritanceFromDacExtension,
					Descriptors.PX1011_NotSealedDacExtension
				);

			protected override void CheckForConstructors(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
			{ }
		}
	}
}
