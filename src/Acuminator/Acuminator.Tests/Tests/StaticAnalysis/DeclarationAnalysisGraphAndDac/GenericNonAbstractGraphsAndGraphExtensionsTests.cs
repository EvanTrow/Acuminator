using System;
using System.Collections.Generic;
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

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraphAndDac
{
	public class GenericNonAbstractGraphsAndGraphExtensionsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new GraphAndGraphExtensionDeclarationAnalyzerForPX1112Tests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new TypeModifiersFix();

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericGraph.cs")]
		public async Task GenericGraph_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(5, 15),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 15),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(15, 22),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(19, 15));

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericGraphExtension.cs")]
		public async Task GenericGraphExtension_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(5, 15),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(10, 15));

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericPartialGraphAndGraphExtension.cs")]
		public async Task GenericPartial_GraphAndGraphExtensions_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(5, 23),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(15, 23));

		#region No Diagnostic Scenarios
		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\AbstractGenericGraphAndGraphExtension.cs")]
		public async Task AbstractGenericGraph_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\NonGenericGraphAndGraphExtension.cs")]
		public async Task NonGenericGraph_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion

		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericGraph.cs", 
						  @"GenericNonAbstractGraphsAndGraphExtensions\GenericGraph_Expected.cs")]
		public async Task Generic_Graph_CodeFix_AddAbstract(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericGraphExtension.cs", 
						  @"GenericNonAbstractGraphsAndGraphExtensions\GenericGraphExtension_Expected.cs")]
		public async Task Generic_GraphExtension_CodeFix_AddAbstract(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData(@"GenericNonAbstractGraphsAndGraphExtensions\GenericPartialGraphAndGraphExtension.cs",
						  @"GenericNonAbstractGraphsAndGraphExtensions\GenericPartialGraphAndGraphExtension_Expected.cs")]
		public async Task GenericPartial_GraphAndGraphExtensions_CodeFix_AddAbstract(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);
		#endregion

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1112Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract
				);

			protected override void CheckIfGraphOrGraphExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, 
																		 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfGraphExtensionInheritsFromNonAbstractGraphExtension(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
			{ }

			protected override void CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graph)
			{ }
		}
	}
}