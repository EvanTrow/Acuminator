# Generate Unit Tests for Acuminator Diagnostic

## TASK
Generate comprehensive unit tests for the `{DIAGNOSTIC_ID}` diagnostic (e.g., PX1114, PX1112, PX1113).

## REQUIREMENTS
- Place tests in the current test class.
- Generate 4-5 unit tests covering key scenarios
- Use existing unit tests in the project as examples for structure and naming
- Use the existing analyzer `{AnalyzerClassName}` for the specified diagnostic. By convetion, analyzer classes have word "Analyzer" in their names.
- Use the existing code fix `{CodeFixClassName}` for the specified diagnostic. By convention, code fix classes have word "Fix" in their names.
- **DO NOT** generate code fix tests if no code fix exists for the diagnostic.
- No code fix exists if the base class of the test class is `DiagnosticVerifier` class instead of `CodeFixVerifier`.
- Follow Acuminator naming conventions exactly

## NAMING CONVENTIONS FOR UNIT TESTS
- Use `NoDiagnostic` suffix for tests that should NOT report the diagnostic
- **DO NOT** use `ShouldNotReportDiagnostic` suffix
- **DO NOT** add `ShouldReportDiagnostic` suffix for positive test cases
- Use descriptive names that clearly indicate the scenario being tested
- Examples:
  - `GraphExtension_InheritingFromAbstractExtension_NoDiagnostic`
  - `GraphExtension_InheritingFromNonAbstractExtension`
  - `RegularGraph_InheritanceScenarios_NoDiagnostic` 

## NAMING CONVENTIONS FOR TESTS SOURCES
- Place test source files in the `Sources` subfolder and its subfolders
- Name test source files descriptively based on the scenario being tested
- Examples:
  - `SealedGraphExtension.cs`
  - `NonSealedGraph.cs`
  - `AbstractGraphExtension.cs`
- Code fix tests use two test source files:
  - `{TestSourceFileName}.cs` for the code before the code fix
  - `{TestSourceFileName}_Expected.cs` for the code expected after the code fix

## TEST STRUCTURE
Each test should:
- Generate C# test source files in the `Sources` subfolder and its subfolders
- Use `[Theory]` and `[EmbeddedFileData(@"FolderName\FileName.cs")]` attributes. The path to the test source file specified in the `EmbeddedFileData` attribute should be relative to the `Sources` folder.
- The path to the test source file specified in the `EmbeddedFileData` attribute should use Windows path separators (`\`).
- The C# string with to the test source file specified in the `EmbeddedFileData` attribute should be C# verbatim string literal (use `@` before the opening quote) if the file is located in a subfolder of the `Sources` folder.
- Be `async Task` methods
- Call `await VerifyCSharpDiagnosticAsync(source)` for no-diagnostic cases
- Call `await VerifyCSharpDiagnosticAsync(source, Descriptors.{DIAGNOSTIC_ID}.CreateFor(line, column))` for positive cases
- Use correct line and column numbers for diagnostic expectations. The correct line and columns are the starting position of the location of the reported diagnostic. 

## TEST CATEGORIES TO INCLUDE
1. **Positive cases** - scenarios that SHOULD trigger the diagnostic
2. **Negative cases** - scenarios that should NOT trigger the diagnostic  
3. **Edge cases** - boundary conditions, inheritance chains, generic types

## ANALYZER SETUP
- The analyzer should be prepared in the unit test file by the user in advance.
The setup code depends on the base class of the test class:
- For analyzers derived from `PXGraphAggregatedAnalyzerBase` Use the following analyzer pattern:
```csharp
protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
	new PXGraphAnalyzer(
		CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(), 
		new {AnalyzerClassName}());
```
- For analyzers derived from `DacAggregatedAnalyzerBase` Use the following analyzer pattern:
```csharp
protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
	new DacAnalyzersAggregator(
		CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(), 
		new {AnalyzerClassName}());
```
- For analyzers derived from `PXDiagnosticAnalyzer` Use the following analyzer pattern:
```csharp
protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
	new {AnalyzerClassName}();
```