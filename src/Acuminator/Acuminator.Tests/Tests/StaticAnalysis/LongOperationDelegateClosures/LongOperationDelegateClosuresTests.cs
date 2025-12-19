using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using AnalyzerResources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationDelegateClosures
{
	public class LongOperationDelegateClosuresTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LongOperationDelegateClosuresAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
																				  .WithSuppressionMechanismDisabled()
																				  .WithRecursiveAnalysisEnabled());
		[Theory]
		[EmbeddedFileData("ClosuresInNonGraph.cs")]
		public Task SetProcessDelegate_ReportOnlyCapturedPassedParameters_NonGraphHelper(string actual)
		{
			string[] formatArgGraphLongRunDelegate = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg,
													  AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			string[] formatArgAdapterLongRunDelegate = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg,
														AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			string[] formatArgsGraphProcDelegate = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, 
													AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg];
			string[] formatArgsAdapterProcDelegate = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, 
													  AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 24, column: 4, formatArgGraphLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgGraphLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 26, column: 4, formatArgsGraphProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgsAdapterProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 29, column: 4, formatArgGraphLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 30, column: 4, formatArgGraphLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 31, column: 4, formatArgAdapterLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 32, column: 4, formatArgGraphLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 33, column: 4, formatArgGraphLongRunDelegate));
		}

		[Theory]
		[EmbeddedFileData("HelperCallsToAnotherFile.cs", "ClosuresInNonGraph.cs")]
		public Task RecursiveAnalysis_CallsTo_NonGraphHelper_FromOtherFile(string actual, string helper)
		{
			string[] formatArgsGraphAndProcDelegate = 
				[AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg];
			string[] formatArgsGraphAndLongRunDelegate =
				[AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			string[] formatArgsAdapterAndProcDelegate =
				[AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg];
			string[] formatArgsAdapterAndLongRunDelegate =
				[AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];

			return VerifyCSharpDiagnosticAsync(actual, helper,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 19, column: 4, formatArgsGraphAndProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 20, column: 4, formatArgsAdapterAndProcDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 22, column: 4, formatArgsGraphAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 24, column: 4, formatArgsGraphAndLongRunDelegate),

				// TODO Should show diagnostic for collection expressions, no diagnosic now due to an old version of Roslyn used, ATR-923
				//Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgsGraphAndLongRunDelegate),

				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgsGraphAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 28, column: 4, formatArgsGraphAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 29, column: 4, formatArgsAdapterAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 30, column: 4, formatArgsGraphAndLongRunDelegate),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 31, column: 4, formatArgsGraphAndLongRunDelegate));
		}

		[Theory]
		[EmbeddedFileData("SetProcessDelegateClosures.cs")]
		public Task SetProcessDelegates_GraphCapturedInClosures(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_ProcessingDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 46, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgs),
				// SetAsyncProcessDelegate tests
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 56, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 57, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_NormalCases.cs")]
		public Task LongRunDelegates_GraphAndAdapterCaptured_NormalCases(string actual)
		{
			string[] formatArgsGraph = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			string[] formatArgsAdapter = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,

				// graph capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 37, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 41, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 59, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 60, column: 4, formatArgsGraph),

				//recursive analysis graph capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 65, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 70, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 71, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 72, column: 4, formatArgsGraph),

				// Test capturing graph via member access
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 80, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 86, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 89, column: 4, formatArgsGraph),

				// Test capturing graph via ILongOperationManager and IGraphLongOperationManager methods
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 92, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 93, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 96, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 97, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 98, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 101, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 102, column: 4, formatArgsGraph),

				// adapter capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 147, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 150, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 151, column: 4, formatArgsAdapter),

				// Test capturing adapter via ILongOperationManager and IGraphLongOperationManager methods
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 162, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 168, column: 4, formatArgsAdapter),

				//recursive analysis adapter capture
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 171, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 173, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 174, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 175, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_ComplexMapping.cs")]
		public Task LongRunDelegates_GraphCaptured_ComplexMappingOfArgumentsToParameters(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				//Params check
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 34, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 36, column: 4, formatArgs),

				// Named parameters check - names in position
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgs),

				// Named parameters check - names out of position
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 48, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 49, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs),

				// Named parameters check - optional parameters
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 54, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 59, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 61, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 62, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("CustomView.cs")]
		public Task CustomView_LongRunCapture_GraphAndAdapter(string actual)
		{
			string[] formatArgsGraph = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			string[] formatArgsAdapter = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 43, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 49, column: 4, formatArgsGraph),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 50, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 52, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("CustomAttribute.cs")]
		public Task CustomAttribute_LongRunCapture_Adapter(string actual)
		{
			string[] formatArgsAdapter = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 33, column: 4, formatArgsAdapter),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 4, formatArgsAdapter));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_Reassign.cs")]
		public Task Adapter_Reassigned_InLongRun(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 39, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 40, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 41, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 42, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 43, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 47, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 50, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 51, column: 4, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_Local.cs")]
		public Task AdapterCaptured_InLocalFunctions_InLongRun(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedPXAdapterFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 25, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 26, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 27, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 28, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 35, column: 5, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("LongRunDelegateClosures_CaptureThis.cs")]
		public Task This_Reference_ToScreenGraph_Captured_InLongRun(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 20, column: 24, formatArgs));
		}

		[Theory]
		[EmbeddedFileData("CircularReferenceInCalls.cs")]
		public Task InterProcedureAnalysis_WithCircularReferences_NoDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory(Skip = "Recursive analysis of passed delegates currently is not supported for this diagnostic and is skipped for now")]
		[EmbeddedFileData("LongRunDelegateClosures_Delegates.cs")]
		public Task GraphDelegateCaptured_InLocalFunctions_InLongRun(string actual)
		{
			string[] formatArgs = [AnalyzerResources.PX1008Title_CapturedGraphFormatArg, AnalyzerResources.PX1008Title_LongRunDelegateFormatArg];
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 26, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 28, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 32, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 38, column: 4, formatArgs),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 41, column: 5, formatArgs));
		}
	}
}
