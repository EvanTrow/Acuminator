using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class DacFieldWithDBCalcedAttributeTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPropertyAttributesAnalyzer());

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndUnboundTypeAttributes.cs")]
		public Task DacField_WithPXDBCalcedAndNonDBAttribute_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithPXDBCalcedAndWithoutUnboundTypeAttributes.cs")]
		public Task DacField_WithPXDBCalced_And_WithoutNonDBAttribute_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source, 
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(19, 28),
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(38, 25),
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute.CreateFor(48, 25));

		[Theory]
		[EmbeddedFileData("DacWithPXDBScalarAndUnboundTypeAttributes.cs")]
		public Task DacField_WithPXDBScalar_And_NonDBAttribute_NoDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacWithPXDBScalarAndWithoutUnboundTypeAttributes.cs")]
		public Task DacField_WithPXDBScalar_And_WithoutNonDBAttribute_ReportsDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(20, 23),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(35, 25),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(44, 27),
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute.CreateFor(53, 25));
	}
}
