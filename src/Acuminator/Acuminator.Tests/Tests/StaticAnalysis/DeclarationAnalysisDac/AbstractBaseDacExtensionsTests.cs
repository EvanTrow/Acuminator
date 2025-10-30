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
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisDac
{
	public class AbstractBaseDacExtensionsTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacAndDacExtensionDeclarationAnalyzerForPX1115Tests());

		[Theory]
		[EmbeddedFileData(@"AbstractBaseDacExtensions\DacExtensionWithNonAbstractBaseExtensions.cs")]
		public Task DacExtension_WithNonAbstractBaseExtensions_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"AbstractBaseDacExtensions\DacExtensionWithAbstractBaseExtensions.cs")]
		public Task DacExtension_WithMultipleAbstractBaseExtensions(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1115_NonTerminalBaseDacExtension.CreateFor(23, 75),
				Descriptors.PX1115_NonTerminalBaseDacExtension.CreateFor(33, 64),
				Descriptors.PX1115_NonTerminalBaseDacExtension.CreateFor(33, 97));

		private sealed class DacAndDacExtensionDeclarationAnalyzerForPX1115Tests : DacAndDacExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1115_NonTerminalBaseDacExtension
				);

			protected override void CheckIfDacExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }

			protected override void ReportDacExtensionInheritance(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
			{ }

			protected override void CheckAttributesDeclaredOnDac(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
			{ }

			protected override void CheckForConstructors(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
			{ }
		}
	}
}
