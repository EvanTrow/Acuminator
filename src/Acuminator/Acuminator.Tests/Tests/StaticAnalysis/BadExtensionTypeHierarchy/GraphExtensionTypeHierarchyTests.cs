using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy;

public class GraphExtensionTypeHierarchyTests : DiagnosticVerifier
{
	protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithStaticAnalysisEnabled()
													.WithSuppressionMechanismDisabled());

	[Theory]
	[EmbeddedFileData(@"Graph\GraphExtension_LinearHierarchy.cs")]
	public Task GraphExtension_WithLinearHierarchy_NoDiagnostic(string actual) =>
		VerifyCSharpDiagnosticAsync(actual);

	[Theory]
	[EmbeddedFileData(@"Graph\GraphExtension_ComplexHierarchy.cs")]
	public Task GraphExtension_WithComplexHierarchy_NoDiagnostic(string actual) =>
		VerifyCSharpDiagnosticAsync(actual);

	[Theory]
	[EmbeddedFileData(@"Graph\GraphExtensionWithCircularSelfReference.cs")]
	public Task GraphExtension_WithCircularSelfReference_InTypeHierarchy(string actual) =>
		VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension.CreateFor(10, 15,
											["ExtensionA", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources.ExtensionA"]));

	[Theory]
	[EmbeddedFileData(@"Graph\GraphExtensionWithComplexCircularReference.cs")]
	public Task GraphExtension_WithComplexCircularReferences_InTypeHierarchy(string actual) =>
		VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension.CreateFor(10, 15,
											["ExtensionA", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources.ExtensionA"]),
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension.CreateFor(13, 15,
											["ExtensionB", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources.ExtensionB"]),
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension.CreateFor(16, 15,
											["ExtensionC", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Graph.Sources.ExtensionC"]));
}