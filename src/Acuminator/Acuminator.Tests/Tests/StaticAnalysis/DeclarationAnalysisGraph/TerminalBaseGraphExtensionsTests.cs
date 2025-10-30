using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph
{
	public class TerminalBaseGraphExtensionsTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new GraphAndGraphExtensionDeclarationAnalyzerForPX1115Tests());

		[Theory]
		[EmbeddedFileData(@"TerminalBaseGraphExtensions\RegularGraph.cs")]
		public Task RegularGraph_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"TerminalBaseGraphExtensions\TerminalBaseGraphExtensions.cs")]
		public Task GraphExtension_With_Terminal_BaseExtensions(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"TerminalBaseGraphExtensions\NonTerminalBaseGraphExtensions.cs")]
		public Task GraphExtension_With_NonTerminal_BaseExtensions(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1115_NonTerminalBaseGraphExtension.CreateFor(26, 56),
				Descriptors.PX1115_NonTerminalBaseGraphExtension.CreateFor(26, 98),
				Descriptors.PX1115_NonTerminalBaseGraphExtension.CreateFor(27, 17),
				Descriptors.PX1115_NonTerminalBaseGraphExtension.CreateFor(32, 69),
				Descriptors.PX1115_NonTerminalBaseGraphExtension.CreateFor(33, 21));

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1115Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1115_NonTerminalBaseGraphExtension
				);

			protected override void CheckIfGraphOrGraphExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, 
																		 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfGraphExtensionInheritsFromTerminalGraphExtension(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
			{ }

			protected override void CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graph)
			{ }

			protected override void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																					 PXGraphEventSemanticModel graphOrGraphExt)
			{ }
		}
	}
}