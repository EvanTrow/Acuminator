using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
	public class LongOperationInPXGraphDuringInitializationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new LongOperationInGraphAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\Initialization\PXGraphStartsLongOperationInInitialization.cs")]
		public Task Graph_Initialization_StartLongOperation(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(19, 5),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(24, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(29, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\Initialization\PXGraphStartsLongOperationInInitializationViaMethod.cs")]
		public Task Graph_Initialization_StartLongOperation_ViaMethod(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(19, 5),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(23, 31),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(28, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\Initialization\PXGraphExtensionStartsLongOperationInInitialization.cs")]
		public Task GraphExtension_Initialization_StartLongOperation(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(17, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(23, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(30, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(31, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(32, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(34, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(48, 5),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(49, 5),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(53, 5),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(56, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(67, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(68, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(69, 4),
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(70, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\Initialization\PXGraphAndGraphExt_NoLongOperationInInitialization.cs")]
		public Task Graph_And_GraphExtension_With_All_Initializers_NoDiagnostic(string source) => 
			VerifyCSharpDiagnosticAsync(source);
	}
}
