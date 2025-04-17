#nullable enable

using System;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.AsyncVoidMethodsAndLambdas;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.AsyncVoidMethodsAndLambdas
{
	public class AsyncVoidMethodsAndLambdasTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new AsyncVoidMethodsAndLambdasAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		#region Method Tests
		[Theory]
		[EmbeddedFileData(@"Methods\NormalAsyncMethods.cs")]
		public async virtual Task NormalAsyncMethods_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Methods\RegularVoidAsyncMethods.cs")]
		public async virtual Task AsyncVoid_RegularMethods(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(10, 16),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(15, 16),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(19, 16));

		[Theory]
		[EmbeddedFileData(@"Methods\PartialVoidAsyncMethods.cs", @"Methods\PartialVoidAsyncMethods.OtherDeclaration.cs")]
		public async virtual Task AsyncVoid_PartialMethodsWithoutBody(string source, string otherDeclaration) =>
			await VerifyCSharpDiagnosticAsync(source, otherDeclaration,
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(7, 18),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(9, 18),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(11, 18));

		[Theory]
		[EmbeddedFileData(@"Methods\PartialVoidAsyncMethods.OtherDeclaration.cs", @"Methods\PartialVoidAsyncMethods.cs")]
		public async virtual Task AsyncVoid_PartialMethodsWithBody(string source, string otherDeclaration) =>
			await VerifyCSharpDiagnosticAsync(source, otherDeclaration,
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(7, 24),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(12, 24),
				Descriptors.PX1038_AsyncVoidMethod.CreateFor(16, 24));
		#endregion

		#region Lambda Tests
		[Theory]
		[EmbeddedFileData(@"Lambdas\NormalAsyncLambdas.cs")]
		public async virtual Task NormalAsyncLambdas_ShouldNotShowDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Lambdas\AsyncVoidLambdas.cs")]
		public async virtual Task AsyncVoid_RegularLambdas(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(10, 21),
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(11, 26),
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(12, 26),
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(13, 26),

				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(15, 27),
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(16, 32),

				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(18, 12),
				Descriptors.PX1038_AsyncVoidLambdasAndAnonymousDelegates.CreateFor(19, 12));
		#endregion
	}
}
