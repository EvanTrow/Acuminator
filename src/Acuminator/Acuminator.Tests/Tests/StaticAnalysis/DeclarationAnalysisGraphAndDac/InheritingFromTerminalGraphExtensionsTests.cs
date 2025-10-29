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
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Tests.Tests.StaticAnalysis.DeclarationAnalysisGraph
{
	public class InheritingFromTerminalGraphExtensionsTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new GraphAndGraphExtensionDeclarationAnalyzerForPX1114Tests());

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\InheritFromAbstractOrGenericGraphExtension.cs")]
		public async Task InheritFrom_Abstract_Or_Generic_GraphExtension_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\RegularGraphInheritance.cs")]
		public async Task RegularGraph_InheritanceScenarios_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\FirstLevelGraphExtension.cs")]
		public async Task FirstLevel_GraphExtension_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\InheritFromNonAbstractExtension.cs")]
		public async Task InheritFrom_NonAbstract_GraphExtension(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension.CreateFor(19, 39));

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\InheritFromNonAbstractExtensionIndirectly.cs",
						  @"InheritingFromTerminalGraphExtensions\InheritFromNonAbstractExtensionIndirectly.BaseTypes.cs")]
		public async Task InheritFrom_NonAbstract_GraphExtension_Indirectly(string source, string baseSource) =>
			await VerifyCSharpDiagnosticAsync(source, baseSource,
				Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension.CreateFor(6, 43));

		[Theory]
		[EmbeddedFileData(@"InheritingFromTerminalGraphExtensions\InheritFromPXProtectedAccessExtension.cs")]
		public async Task GraphExtension_InheritingFromPXProtectedAccessExtension(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension.CreateFor(19, 47));

		private sealed class GraphAndGraphExtensionDeclarationAnalyzerForPX1114Tests : GraphAndGraphExtensionDeclarationAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
				ImmutableArray.Create
				(
					Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension
				);

			protected override void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																					 PXGraphEventSemanticModel graphOrGraphExt)
			{
			}

			protected override void CheckIfGraphOrGraphExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, 
																		 PXGraphEventSemanticModel graphOrGraphExt)
			{ }

			protected override void CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(SymbolAnalysisContext context, PXContext pxContext, 
																				SemanticModel? semanticModel, PXGraphEventSemanticModel graph)
			{ }
		}
	}
}