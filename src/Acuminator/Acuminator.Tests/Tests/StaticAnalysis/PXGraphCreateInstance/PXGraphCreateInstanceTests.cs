using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	public class PXGraphCreateInstanceTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXGraphCreateInstanceFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphCreateInstanceAnalyzer(CodeAnalysisSettings.Default
																  .WithStaticAnalysisEnabled()
																  .WithSuppressionMechanismDisabled());

		[Theory]
		[EmbeddedFileData("Method.cs")]
		public Task CallTo_Constructor_In_Method(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1001_PXGraphCreateInstance.CreateFor(15, 16),
				Descriptors.PX1001_PXGraphCreateInstance.CreateFor(16, 31));

		[Theory]
		[EmbeddedFileData("Field.cs")]
		public Task CallTo_Constructor_In_FieldInitializer(string actual) => 
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1001_PXGraphCreateInstance.CreateFor(12, 43));

		[Theory]
		[EmbeddedFileData("Property.cs")]
		public Task CallTo_Constructor_In_Property(string actual) => 
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1001_PXGraphCreateInstance.CreateFor(14, 17));

		[Theory]
		[EmbeddedFileData("MethodWithNonSpecificPXGraph.cs")]
		public Task CallTo_NonSpecificPXGraph_Constructor_In_Method(string actual) => 
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1003_NonSpecificPXGraphCreateInstance.CreateFor(14, 16));

		[Theory]
		[EmbeddedFileData("Method.cs", "Method_Expected.cs")]
		public Task CodeFix_ReplaceConstructorCall_In_Method(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("Field.cs", "Field_Expected.cs")]
		public Task CodeFix_ReplaceConstructorCall_In_FieldInitializer(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("Property.cs", "Property_Expected.cs")]
		public Task CodeFix_ReplaceConstructorCall_In_Property(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);
	}
}
