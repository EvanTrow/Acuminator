using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges
{
	public class SavingChangesInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LooseEventHandlerAggregatorAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new SavingChangesInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSave.cs")]
		public Task PressSave(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\SavePress.cs")]
		public Task SavePress(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\GraphPersist.cs")]
		public Task GraphPersist(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CachePersist.cs")]
		public Task CachePersist(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(19, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(24, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(29, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(30, 4),
				Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(31, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSaveInExternalMethod.cs")]
		public Task PressSaveInExternalMethod(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1043_SavingChangesInEventHandlers.CreateFor(14, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressSaveInsideRowPersisting.cs")]
		public Task PressSaveInsideRowPersisting(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1043_SavingChangesInRowPerstisting.CreateFor(14, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CachePersistInsideRowPersisting.cs")]
		public Task CachePersistInsideRowPersisting_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ArbitraryCodeInsideRowPersisting.cs")]
		public Task ArbitraryCodeInsideRowPersisting_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
	}
}
