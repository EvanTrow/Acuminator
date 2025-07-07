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
				new LongOperationInDataViewDelegateAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperation.cs")]
		public Task DataViewDelegateFromGraph_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataViewFromGraphStartsLongOperationViaMethod.cs")]
		public Task DataViewDelegateWithMethodFromGraph_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(17, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataViewFromGraphExtensionStartsLongOperation.cs")]
		public Task DataViewDelegateFromGraphExtension_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));

		[Theory]
		[EmbeddedFileData(@"PXGraph\DataViewWithParameterStartsLongOperation.cs")]
		public Task DataViewDelegateWithParameter_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(19, 17));

		[Theory]
		[EmbeddedFileData(@"PXGraph\StaticDataViewStartsLongOperation.cs")]
		public Task StaticDataViewDelegate_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, Descriptors.PX1080_DataViewDelegateLongOperationStart.CreateFor(15, 13));
	}
}
