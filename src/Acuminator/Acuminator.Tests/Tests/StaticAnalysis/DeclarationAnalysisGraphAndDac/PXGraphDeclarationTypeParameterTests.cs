using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraphAndDac
{
	public class PXGraphDeclarationTypeParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new GraphAndGraphExtensionDeclarationAnalyzerForPX1093Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new PXGraphDeclarationTypeParameterFix();

		[Theory]
		[EmbeddedFileData(@"GraphDeclarationTypeParameter\Graph_Bad.cs")]
		public async Task Graph_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(11, 35),
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(15, 39),
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(19, 33));

		[Theory]
		[EmbeddedFileData(@"GraphDeclarationTypeParameter\Graph_Good.cs")]
		public async Task Graph_AfterCodeFix_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"GraphDeclarationTypeParameter\Graph_Bad.cs", @"GraphDeclarationTypeParameter\Graph_Good.cs")]
		public async Task Graph_IncorrectTypeArgument_In_BaseType_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1093Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1093_GraphDeclarationViolation
				);

			protected override void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																					 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfGraphOrGraphExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext,
																		 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfGraphExtensionInheritsFromNonAbstractGraphExtension(SymbolAnalysisContext context, PXContext pxContext, 
																				SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
			{ }
		}
	}
}
