using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph
{
	public class SealedGraphsAndGraphExtensionsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new GraphAndGraphExtensionDeclarationAnalyzerForPX1113Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new TypeModifiersFix();

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraph.cs")]
		public Task SealedGraph(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(5, 9),
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(9, 9),
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(15, 9),
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(19, 9),
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(23, 9));

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraphExtension.cs")]
		public Task SealedGraphExtension(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(6, 9),
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.CreateFor(11, 9));

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\RegularGraphAndGraphExtension.cs")]
		public Task Regular_GraphsAndGraphExtensions_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraph_Expected.cs")]
		public Task SealedGraph_AfterCodeFix_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraphExtension_Expected.cs")]
		public Task SealedGraphExtension_AfterCodeFix_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraph.cs", 
						  @"SealedGraphsAndGraphExtensions\SealedGraph_Expected.cs")]
		public Task Sealed_Graph_CodeFix_RemoveSealed(string source, string expected) =>
			VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"SealedGraphsAndGraphExtensions\SealedGraphExtension.cs", 
						  @"SealedGraphsAndGraphExtensions\SealedGraphExtension_Expected.cs")]
		public Task Sealed_GraphExtension_CodeFix_RemoveSealed(string source, string expected) =>
			VerifyCSharpFixAsync(source, expected);
		#endregion

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1113Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1113_SealedGraphsAndGraphExtensions
				);

			protected override void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																					 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfGraphExtensionInheritsFromTerminalGraphExtension(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
			{ }

			protected override void CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graph)
			{ }

			protected override void CheckIfGraphExtensionHasNonTerminalBaseExtensions(SymbolAnalysisContext context, PXContext pxContext,
																				SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
			{ }
		}
	}
}