
#nullable enable
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldTest : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("LegacyBqlFieldGood.cs")]
		public async Task Dac_WithTypedBqlFields_ShouldNotShowDiagnostic(string actual) => await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("LegacyBqlFieldBad.cs")]
		public async Task Dac_WithLegacyBqlFields(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1060_LegacyBqlField.CreateFor(13, 25, "legacyBoolField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(17, 25, "legacyByteField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(21, 25, "legacyShortField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(25, 25, "legacyIntField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(29, 25, "legacyLongField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(33, 25, "legacyFloatField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(37, 25, "legacyDoubleField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(41, 25, "legacyDecimalField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(45, 25, "legacyStringField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(49, 25, "legacyDateField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(53, 25, "legacyGuidField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(57, 25, "legacyBinaryField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(61, 25, "attributes"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(68, 25, "legacyNullableStringField"),
				Descriptors.PX1060_LegacyBqlField.CreateFor(75, 25, "legacyNullableBinaryField"));

		[Theory]
		[EmbeddedFileData("LegacyBqlFieldBad.cs", "LegacyBqlFieldBad_Expected.cs")]
		public async Task Test_UpgradeBqlFields_CodeFix(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new LegacyBqlFieldAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new LegacyBqlFieldFix();
	}
}