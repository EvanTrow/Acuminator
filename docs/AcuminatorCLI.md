# Acuminator Console Runner

The **Acuminator console runner** is a standalone command-line tool to perform Acuminator static code analysis of .NET projects and solutions based on the Acumatica Framework.
Acuminator console runner serves as a command-line interface (CLI) for Acuminator code analysis that allows to run it outside of IDE. Such tool is useful for CI/CD pipelines and other automated scenarios.
The name of the executable file is `Acuminator.Runner.NetFramework.exe`.

The Acuminator console runner supports analysis of .Net solutions (*.sln*) and projects (*.csproj*). The tool requires .Net Framework 4.8 runtime.

## Analysis

Acuminator enforces Acumatica-specific development rules and best practices, helping developers to ensure that their code adheres to the standards required for Acumatica customization and extension.
Acuminator diagnostics are designed to help developers avoid common mistakes, enforce architectural guidelines, and ensure compatibility with Acumatica’s extensibility model. 

Among the things checked by Acuminator are:
- **Graphs and DACs:** Acuminator checks that Acumatica Graphs and DACs follow Acumatica’s required best practices and conventions.
- **DAC Field Attributes:** Acuminator checks for proper declaration of Acumatica attributes declared on DAC field properties.
- **Event Handlers:** Acuminator verifies correct usage of Graph event handler methods (e.g., ``RowSelected``, ``FieldUpdated``).
- **Prohibited API Usage:** Acuminator detects usage of Acumatica internal APIs or APIs that are discouraged or unsupported in Acumatica customizations.

You can find the list of all Acuminator diagnostics in this [summary](./Summary.md).

Acuminator also provides its own mechanism for diagnostics suppression, which allows developers to selectively suppress specific diagnostics in their code. This is useful when a particular diagnostic is not applicable or when a developer has a valid reason
to ignore it. The suppression mechanism can be applied via comments in the code or through special Acuminator suppression files.

## Command Line Arguments

The Acuminator console runner provides several command line arguments to configure its code analysis and the format of the generated report. Run the tool with `--help` argument to see the documentation for all supported command line arguments in the console.

All command line arguments can be divided into three groups:
- Code analysis arguments: these arguments control how the code analysis is performed.
- Output arguments: these arguments control how the results of the code analysis are reported.
- Other arguments: these are additional arguments that control different aspects of the Acuminator console runner.

### Analysis Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                                                   |
|--------------------------------------- |-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `codeSource` (position 0)              | Required argument that should be specified first. A path to the "code source" which will be validated. The term "code source" is a general term to describe something that can provide source code to the tool. Currently, the supported code sources are C# projects and C# solutions. |
| `--noSuppression`                      | When this optional flag is specified, the code analysis will report Acuminator errors suppressed with Acuminator suppression mechanisms.                                                                                                                                                |
| `--isvMode`                            | This optional flag enables ISV-specific analysis mode. In this mode Acuminator performs stricter code analysis with extra diagnostics. The severity of some diagnostics is also increased from "Warning" to "Error". The name of this mode is inspired by the Acumatica certification process for customizations developed by Independent Software Vendors (ISV). The certification uses stricter Acuminator analysis in the ISV mode to validate such customizations. |
| `--enable-PX1007`                      | This optional flag enables the [**PX1007**](./diagnostics/PX1007.md) diagnostic. This diagnostic checks the presence of XML documentation comments on DACs and DAC fields. It is used internally in Acumatica development processes but disabled by default.                  |
| `--disable-PX1099`                     | Disables the [**PX1099**](./diagnostics/PX1099.md) diagnostic. The **PX1099** diagnostic detects APIs that should not be used with the Acumatica Framework.                                                                                                                   |
| `--bannedAPIs`                         | Path to a file with a custom list of banned APIs for the **PX1099** diagnostic. Read more about custom files with banned APIs and the format of banned APIs in [**PX1099** documentation](./diagnostics/PX1099.md).                                                           |
| `--allowedAPIs`                        | Path to a file with a custom list of allowed APIs for the **PX1099** diagnostic. Read more about allowed APIs, custom files with allowed APIs, and the format of allowed APIs in [**PX1099** documentation](./diagnostics/PX1099.md).                                         |
| `--generateSuppressionFile`            | When this optional flag is specified, Acuminator should work in a special suppression file generator mode. In this mode Acuminator does not report errors, but instead generates suppression records in Acuminator suppression files for all errors it will find in the code. |


### Output Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                     |
|--------------------------------------- |-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-f`, `--file`                         | The path to the output file. If not specified then the report with analysis results will be outputted to the console window.                                                                                                                    |
| `--outputAbsolutePaths`                | This flag regulates how the locations of errors will be outputted. By default, the report will contain file paths relative to the containing project directory. However, if this flag is set, then the absolute file paths will be used.        |
| `--format`                             | The report output format. There are two supported values: `text` (default) or `json`.                                                                                                                                                           |
| `-g`, `--grouping`                     | Groups diagnostics in the report. The expected values are combinations of symbols `f`/`F` (files) and `d`/`D` (diagnostic IDs). Example: `-g fd` groups found errors by file and diagnostic ID.                                                 |

### Other Command Line Arguments

| Argument                               | Description                                                                                                                                                                                                                                                                                            |
|--------------------------------------- |--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `-v`, `--verbosity`                    | This optional parameter allows you to explicitly specify logger verbosity. The allowed values are taken from the `Serilog.Events.LogEventLevel` enum. The allowed values: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`. By default, the logger will use the `Information` verbosity. |
| `--msBuildPath`                        | This optional parameter allows you to provide explicitly a path to the MSBuild tool that will be used for analysis. By default, MSBuild installations will be searched automatically on the current machine and the latest found version will be used.                                                 |
| `--help`                               | Specify this flag without any other arguments to see the description of all available command line arguments in the console.                                                                                                                                                                           |
| `--version`                            | Specify this flag without any other arguments to see Acuminator console runner's version.                                                                                                                                                                                                              |

## Usage Examples

Below are examples of how you can call Acuminator console runner from the command line:
```console
Acuminator.Runner.NetFramework.exe <path to solution/project> --verbosity Debug --format json -f <path to output file> -g <grouping> --enable-PX1007 --disable-PX1099   
```
The example with real values will look like this:
```console
Acuminator.Runner.NetFramework.exe "..\..\..\..\..\Samples\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo.csproj" --verbosity Debug --format json -f report.json -g FD --enable-PX1007 --disable-PX1099
```
The command above will analyze the `PX.Objects.HackathonDemo` project, output the report in JSON format to the `report.json` file, group diagnostics by file and diagnostic ID, enable the **PX1007** diagnostic, and disable the **PX1099** diagnostic.
The verbosity of the logger will be set to `Debug`.

To run Acuminator analysis in a plain text format with default code analysis settings and output it to console, you can use the following command:
```console
Acuminator.Runner.NetFramework.exe "..\..\..\..\..\Samples\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo\PX.Objects.HackathonDemo.csproj"
```
This command does not specify any grouping for diagnostics, so they will be output in a flat ordered list for each analyzed project. Note, that if you run the analysis for the C# solution, diagnostics will always be grouped by project. 

## Acuminator Console Runner and Different .Net Runtimes

Currently, there is only one version of Acuminator console runner based on .Net Framework runtime. The runner is based on the older .Net Framework runtime due to differences in Roslyn and MSBuild behavior for console applications 
based on different .Net runtimes. These differences can be observed on large complex code bases such as Acumatica's code. 

In case of modern .Net runtimes, Roslyn (and MSBuild used by the tool to load solutions for analysis) fails to correctly load Acumatica solution for analysis. This results in the inability to correctly analyze Acumatica projects. 
On the other hand, when Roslyn and MSBuild are used with .Net Framework, they can correctly load Acumatica solution and projects for analysis.

It seems, the issue is related to the fact that Acumatica solution is still based on .Net Framework, which causes compatibility problems with Roslyn and MSBuild dlls that target different .Net runtimes. In the future, after Acumatica
is fully migrated to .Net Core, a port of Acuminator console runner to .Net Core will probably appear.