using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheDuringPXGraphInitializationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled(),
				new ChangesInPXCacheDuringPXGraphInitializationAnalyzer());

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphChangesPXCache.cs")]
		public void Graph_Initialization_ChangesInPXCache(string source)
		{
			VerifyCSharpDiagnostic(source,
				// Constructor
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(16, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(17, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(18, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(20, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(21, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(22, 5),

				// Initialize
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(32, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(33, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(34, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(36, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(37, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(38, 5),

				// Configure
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(50, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(51, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(52, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(54, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(55, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(56, 5)
			);
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphChangesPXCacheViaMethod.cs")]
		public void Graph_Initialization_ChangesInPXCache_ViaCalledMethod(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(17, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(21, 31),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(26, 4));
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphExtensionChangesPXCache.cs")]
		public void GraphExtension_Initialization_ChangesInPXCache(string source)
		{
			VerifyCSharpDiagnostic(source,
				// Constructor
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(16, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(17, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(18, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(20, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(21, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(22, 5),

				// Initialize
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(32, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(33, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(34, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(36, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(37, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(38, 5),

				// Configure
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(50, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(51, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(52, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(54, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(55, 5),
				Descriptors.PX1059_ChangesInPXCacheDuringPXGraphInitialization.CreateFor(56, 5));
		}

		[Theory]
		[EmbeddedFileData(@"PXGraph\PXGraphDoesntChangePXCache.cs")]
		public void Graph_Initialization_DoesNotChangePXCache_DoesntReportsDiagnostic(string source) => 
			VerifyCSharpDiagnostic(source);
	}
}
