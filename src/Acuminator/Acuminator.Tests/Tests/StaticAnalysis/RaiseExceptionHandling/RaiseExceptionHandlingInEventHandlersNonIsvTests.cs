using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.RaiseExceptionHandling;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.RaiseExceptionHandling
{
	public class RaiseExceptionHandlingInEventHandlersNonIsvTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LooseEventHandlerAggregatorAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new RaiseExceptionHandlingInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public async Task EventHandlers(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(19, 4, EventType.FieldSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(34, 4, EventType.FieldUpdating),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(39, 4, EventType.FieldUpdating));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public async Task EventHandlersWithExternalMethod(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(19, 4, EventType.FieldSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(34, 4, EventType.FieldUpdating),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV.CreateFor(39, 4, EventType.FieldUpdating));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public async Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}