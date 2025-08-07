#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ObsoleteElementsUsage;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ObsoleteElementsUsage
{
	public class ObsoleteElementsUsageTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new ObsoleteElementsUsageAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXOverride\PXOverrideOfObsoleteMethods.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegate_Parameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1100_PXOverrideOverridesObsoleteMethod.CreateFor(12, 17),
				Descriptors.PX1100_PXOverrideOverridesObsoleteMethod.CreateFor(18, 15),
				Descriptors.PX1100_PXOverrideOverridesObsoleteMethod.CreateFor(23, 15),
				Descriptors.PX1100_PXOverrideOverridesObsoleteMethod.CreateFor(29, 15),
				Descriptors.PX1100_PXOverrideOverridesObsoleteMethod.CreateFor(39, 17));
	}
}