using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
	public class LongOperationInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LooseEventHandlerAggregatorAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new LongOperationInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Invalid.cs")]
		public Task TestDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(17, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(18, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(23, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(24, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(29, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(31, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(36, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(37, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(42, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(43, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(52, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(53, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(58, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(59, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(68, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(69, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(74, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(75, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(80, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(81, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(86, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(87, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(92, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(93, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\InvalidWithExternalMethod.cs")]
		public Task TestDiagnostic_WithExternalMethod(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(14, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(15, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(16, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(17, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(18, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(23, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(24, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(25, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(26, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(27, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(32, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(33, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(34, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(35, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(36, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(41, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(42, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(43, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(44, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(45, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(50, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(51, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(52, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(53, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(54, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(59, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(60, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(61, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(62, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(63, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(68, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(69, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(70, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(71, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(72, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(77, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(78, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(79, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(80, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(81, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(86, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(87, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(88, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(89, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(90, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(95, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(96, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(97, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(98, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(99, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(104, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(105, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(106, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(107, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(108, 4),

				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(113, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(114, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(115, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(116, 4),
				Descriptors.PX1046_LongOperationInEventHandlers.CreateFor(117, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Valid.cs")]
		public Task TestDiagnostic_EventHandlers_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
	}
}
