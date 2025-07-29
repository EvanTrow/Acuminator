using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzer());

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsExactlyMatch.cs")]
		public void ArgumentsExactlyMatch(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatch.cs")]
		public void ArgumentsDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\LastArgumentIsNotDelegate.cs")]
		public void LastArgumentIsNotDelegate(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22),
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(23, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentTypesDoNotMatch.cs")]
		public void ArgumentTypesDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ReturnTypesDoNotMatch.cs")]
		public void ReturnTypesDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 23)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatchWithDelegate.cs")]
		public void ArgumentsDoNotMatchWithDelegate(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 25)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatchBaseHasMoreParameters.cs")]
		public void ArgumentsDoNotMatchBaseHasMoreParameters(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseMethodIsNotVirtual.cs")]
		public void BaseMethodIsNotVirtual(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseMethodIsNotAccessible.cs")]
		public void BaseMethodIsNotAccessible(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DerivedMethodIsGeneric.cs")]
		public void DerivedMethodIsGeneric(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 15)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DerivedMethodIsStatic.cs")]
		public void DerivedMethodIsStatic(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 21)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsMatchWithDelegate.cs")]
		public void ArgumentsMatchWithDelegate(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DelegateSignatureDoesNotMatch.cs")]
		public void DelegateSignatureDoesNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 25)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\PxOverrideInADifferentType.cs")]
		public void PxOverrideInADifferentType(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 26)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ParentMethodAlsoHasTheDelegateSignature.cs")]
		public void ParentMethodAlsoHasTheDelegateSignature(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\NoOverridenMethodDoesNotCrash.cs")]
		public void NoOverridenMethodDoesNotCrash(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\NoPxOverrideAttributeDoNotCrash.cs")]
		public void NoPxOverrideAttributeDoNotCrash(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\MethodHasTheDelegateAsType.cs")]
		public void MethodHasTheDelegateAsType(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\MethodHasTheDelegateAsTypeAndReturnsVoid.cs")]
		public void MethodHasTheDelegateAsTypeAndReturnsVoid(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeImplementsPxGraphExtension.cs")]
		public void BaseTypeImplementsPxGraphExtension(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeImplementsPxGraphExtensionSignatureIsWrong.cs")]
		public void BaseTypeImplementsPxGraphExtensionSignatureIsWrong(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(21, 25)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeDefinedAsExtension.cs")]
		public void BaseTypeDefinedAsExtension(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(28, 26)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeDefinedAsExtensionNoError.cs")]
		public void BaseTypeDefinedAsExtensionNoError(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\OverridenMethodIsInTheBaseOfTheBaseExtension.cs")]
		public void OverridenMethodIsInTheBaseOfTheBaseExtension(string source) => VerifyCSharpDiagnostic(source);
	}
}