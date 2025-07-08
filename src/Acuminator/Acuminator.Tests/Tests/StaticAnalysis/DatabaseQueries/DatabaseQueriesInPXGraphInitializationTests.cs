using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using System.Threading.Tasks;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries
{
	public class DatabaseQueriesInPXGraphInitializationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
				.WithRecursiveAnalysisEnabled()
				.WithIsvSpecificAnalyzersEnabled(),
				new DatabaseQueriesInPXGraphInitializationAnalyzer());

		[Theory]
		[EmbeddedFileData(@"Initializers\GraphInitialization.cs")]
		public Task DbQuery_In_Graph_Initialization(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\GraphExtensionInitialization.cs")]
		public Task DbQuery_In_GraphExtension_Initialization(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\CreateInstanceDelegate.cs")]
		public Task CreateInstanceDelegate_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(20, 25));

		[Theory]
		[EmbeddedFileData(@"Initializers\BQLSelect.cs")]
		public Task BQLSelect_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\BQLSearch.cs")]
		public Task BQLSearch_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\DataView.cs")]
		public Task DataView_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\PXView.cs")]
		public Task PXView_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\PXSelector.cs")]
		public Task PXSelector_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 12),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 12),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 12));

		[Theory]
		[EmbeddedFileData(@"Initializers\PXDatabase.cs")]
		public Task PXDatabase_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\ExternalMethod.cs")]
		public Task ExternalMethod_CausesDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(22, 13),
				Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(28, 13));

		[Theory]
		[EmbeddedFileData(@"Initializers\NonDbCrudOperations.cs")]
		public Task NonDbCrudOperations_DoesntCauseDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
	}
}
