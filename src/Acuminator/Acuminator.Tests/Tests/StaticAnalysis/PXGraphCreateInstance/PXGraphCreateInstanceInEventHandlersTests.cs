using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreateInstance
{
	public class PXGraphCreateInstanceInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LooseEventHandlerAggregatorAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithStaticAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new PXGraphCreateInstanceInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\CreateInstance.cs")]
		public Task CallTo_CreateInstance_InEventHandler(string actual) => 
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(16, 21));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Constructor.cs")]
		public Task CallTo_Constructor_InEventHandler(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(17, 21),
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(18, 17));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ConstructorForNonSpecificPXGraph.cs")]
		public Task CallTo_ConstructorForNonSpecificPXGraph_InEventHandler(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(17, 16),
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers.CreateFor(18, 12));
	}
}