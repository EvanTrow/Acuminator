using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy;

public class DacExtensionTypeHierarchyTests : DiagnosticVerifier
{
	protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(CodeAnalysisSettings.Default
														   .WithStaticAnalysisEnabled()
														   .WithSuppressionMechanismDisabled());

	[Theory]
	[EmbeddedFileData(@"Dac\DacExtension_LinearHierarchy.cs")]
	public Task DacExtension_WithLinearHierarchy_NoDiagnostic(string actual) =>
		VerifyCSharpDiagnosticAsync(actual);

	[Theory]
	[EmbeddedFileData(@"Dac\DacExtensionWithComplexCircularReference.cs")]
	public Task DacExtension_WithCircularReferences_InTypeHierarchy(string actual) =>
		VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_DacExtension.CreateFor(11, 22,
											["ExtensionA", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Dac.Sources.ExtensionA"]),
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_DacExtension.CreateFor(15, 22,
											["ExtensionB", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Dac.Sources.ExtensionB"]),
			Descriptors.PX1116_CircularReferenceInTypeHierarchy_DacExtension.CreateFor(19, 22,
											["ExtensionC", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Dac.Sources.ExtensionC"]));

	[Theory]
	[EmbeddedFileData(@"Dac\DacExtensionWithForbiddenHierarchy.cs")]
	public Task DacExtension_WithMultiple_Independent_BaseDacExtensions_InTypeHierarchy(string actual) =>
		VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1117_DacExtensionWithComplexTypeHierarchy.CreateFor(11, 22,
							["SecondLevelDacExtension", "Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Dac.Sources.SecondLevelDacExtension"]));
}