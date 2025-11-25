using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField;

public class MainDacOfProcessingViewMustContainNoteIdFieldTests : DiagnosticVerifier
{
	protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
		new PXGraphAnalyzer(
			CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled(),
			new MainDacOfProcessingViewMustContainNoteIdFieldAnalyzer());

	[Theory]
	[EmbeddedFileData("ProcessingViewWithDacMissingNoteId.cs")]
	public Task ProcessingView_MainDac_WithoutNoteId(string source) =>
		VerifyCSharpDiagnosticAsync(source,
			Descriptors.PX1111_MainDacOfProcessingViewMustContainNoteIdField
					   .CreateFor(9, 31, "OrderDac", "ProcessOrders1"),
			Descriptors.PX1111_MainDacOfProcessingViewMustContainNoteIdField
					   .CreateFor(35, 31, "OrderDac", "ProcessOrders2"));

	[Theory]
	[EmbeddedFileData("CustomProcessingViewWithDacMissingNoteId.cs")]
	public Task CustomProcessingView_MainDac_WithoutNoteId(string source) =>
		VerifyCSharpDiagnosticAsync(source,
			Descriptors.PX1111_MainDacOfProcessingViewMustContainNoteIdField
					   .CreateFor(16, 10, "OrderDac", "ProcessOrders"));

	[Theory]
	[EmbeddedFileData("ProcessingViewWithDacHavingNoteId.cs")]
	public Task ProcessingView_MainDac_WithNoteId_NoDiagnostic(string source) =>
		VerifyCSharpDiagnosticAsync(source);

	[Theory]
	[EmbeddedFileData("NonProcessingViewWithDacMissingNoteId.cs")]
	public Task NonProcessingView_MainDac_WithoutNoteId_NoDiagnostic(string source) =>
		VerifyCSharpDiagnosticAsync(source);

	[Theory]
	[EmbeddedFileData("ProcessingViewWithDacInheritingNoteIdFromBase.cs")]
	public Task ProcessingView_MainDac_InheritingNoteId_FromBaseDac_NoDiagnostic(string source) =>
		VerifyCSharpDiagnosticAsync(source);
}