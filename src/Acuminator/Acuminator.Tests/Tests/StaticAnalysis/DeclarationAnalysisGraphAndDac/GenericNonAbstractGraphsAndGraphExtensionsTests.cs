using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraphAndDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

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
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraph.cs")]
		public async Task GenericGraph_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(5, 15),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 15),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(15, 22),
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(19, 15));

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraph.cs")]
		public async Task GenericPartialGraph_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 22, "GenericPartialGraph"));

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtension.cs")]
		public async Task GenericGraphExtension_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 15, "GenericGraphExtension"));

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtensionWithMultipleTypeParameters.cs")]
		public async Task GenericGraphExtension_WithMultipleTypeParameters_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 15, "GenericGraphExtensionWithMultipleTypeParameters"));

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraphExtension.cs")]
		public async Task GenericPartialGraphExtension_NonAbstract(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.CreateFor(9, 22, "GenericPartialGraphExtension"));

		#region No Diagnostic Scenarios

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/AbstractGenericGraph.cs")]
		public async Task AbstractGenericGraph_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/AbstractGenericGraphExtension.cs")]
		public async Task AbstractGenericGraphExtension_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/NonGenericGraph.cs")]
		public async Task NonGenericGraph_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/NonGenericGraphExtension.cs")]
		public async Task NonGenericGraphExtension_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/AbstractPartialGenericGraph.cs")]
		public async Task AbstractPartialGenericGraph_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion

		#region Code Fix Tests

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraph.cs", "GenericNonAbstractGraphsAndGraphExtensions/GenericGraph_Expected.cs")]
		public async Task GenericGraph_CodeFix(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraph.cs", "GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraph_Expected.cs")]
		public async Task GenericPartialGraph_CodeFix(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtension.cs", "GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtension_Expected.cs")]
		public async Task GenericGraphExtension_CodeFix(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtensionWithMultipleTypeParameters.cs", "GenericNonAbstractGraphsAndGraphExtensions/GenericGraphExtensionWithMultipleTypeParameters_Expected.cs")]
		public async Task GenericGraphExtension_WithMultipleTypeParameters_CodeFix(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		[Theory]
		[EmbeddedFileData("GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraphExtension.cs", "GenericNonAbstractGraphsAndGraphExtensions/GenericPartialGraphExtension_Expected.cs")]
		public async Task GenericPartialGraphExtension_CodeFix(string source, string expected) =>
			await VerifyCSharpFixAsync(source, expected);

		#endregion

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1112Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract
				);
		}
	}
}