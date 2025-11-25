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
	public class LongOperationInDataViewDelegateTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new LongOperationInGraphAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataView\DataViewFromGraphStartsLongOperation.cs")]
		public Task DataViewDelegateFromGraph_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(19, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(21, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(22, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(23, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(24, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(26, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(27, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(29, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(30, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(36, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(37, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataView\DataViewFromGraphStartsLongOperationViaMethod.cs")]
		public Task DataViewDelegateWithMethodFromGraph_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(20, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(21, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(22, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(23, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(24, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataView\DataViewFromGraphExtensionStartsLongOperation.cs")]
		public Task DataViewDelegateFromGraphExtension_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, 
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(18, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(20, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(21, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(23, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(24, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(25, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(26, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(28, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(29, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(35, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(36, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataView\DataViewWithParameterStartsLongOperation.cs")]
		public Task DataViewDelegateWithParameter_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(22, 5),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(24, 5),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(25, 5),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(26, 5),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(27, 5),

				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(30, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(31, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(33, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(34, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(40, 4),
				Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(41, 4));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataView\StaticDataViewStartsLongOperation.cs")]
		public Task StaticDataViewDelegate_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));
	}
}
