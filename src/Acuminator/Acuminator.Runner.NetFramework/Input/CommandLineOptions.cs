using System;
using System.Collections.Generic;
using System.Linq;

using Serilog.Events;

using CommandLine;

using Acuminator.Runner.Constants;

namespace Acuminator.Runner.Input
{
	internal class CommandLineOptions
	{
		/// <summary>
		/// The code source that will be analysed by Acuminator.
		/// </summary>
		/// <remarks>
		/// Currently, the supported code sources are C# projects and C# solutions.
		/// </remarks>
		[Value(index: 0, MetaName = CommandLineArgNames.CodeSource, Required = true,
			   HelpText = """
			A path to the "code source" which will be validated. The term "code source" is a generalization for components/services that can provide source code to the tool.
			Currently, the supported code sources are C# projects and C# solutions.
			""")]
		public string CodeSource { get; }

		/// <summary>
		/// Optional explicitly specified logger <see cref="LogEventLevel"/> verbosity for tool's own messages. <br/>
		/// If null then <see cref="LogEventLevel.Information"/> will be used as default.
		/// </summary>
		/// <value>
		/// The explicitly specified logger's verbosity.
		/// </value>
		[Option(shortName: CommandLineArgNames.VerbosityShort, longName: CommandLineArgNames.VerbosityLong,
				HelpText = "This optional parameter allows you to explicitly specify logger verbosity. The allowed values are taken from the " + 
						  $"\"{nameof(Serilog)}.{nameof(Serilog.Events)}.{nameof(LogEventLevel)}\" enum.\r\n" +
						  $"""

						  The allowed values:
						   - "{nameof(LogEventLevel.Verbose)}", 
						   - "{nameof(LogEventLevel.Debug)}", 
						   - "{nameof(LogEventLevel.Information)}",
						   - "{nameof(LogEventLevel.Warning)}",
						   - "{nameof(LogEventLevel.Error)}",
						   - "{nameof(LogEventLevel.Fatal)}".

						  By default, the logger will use the "{nameof(LogEventLevel.Information)}" verbosity."
						  """)]
		public string? Verbosity { get; }

		/// <summary>
		/// If this flag is set to true then the code analysis won't take into consideration suppression comments present in the code.
		/// </summary>
		[Option(longName: CommandLineArgNames.DisableSuppressionMechanism,
				HelpText = "When this optional flag is specified, the code analysis will report Acuminator errors suppressed with Acuminator suppression mechanisms.")]
		public bool DisableSuppressionMechanism { get; }

		/// <summary>
		/// Optional explicitly specified path to MSBuild. Can be null. If null then MSBuild path is retrieved automatically.
		/// </summary>
		/// <value>
		/// The optional explicitly specified path to MSBuild.
		/// </value>
		[Option(longName: CommandLineArgNames.MSBuildPath,
				HelpText = """
						   This optional parameter allows you to provide explicitly a path to the MSBuild tool that will be used for analysis.
						   By default, MSBuild installations will be searched automatically on the current machine and the latest found version will be used.
						   """)]
		public string? MSBuildPath { get; }

		/// <summary>
		/// The name of the output file. If not specified then the report with analysis results will be outputted to the console window.
		/// </summary>
		[Option(shortName: CommandLineArgNames.OutputFileShort, longName: CommandLineArgNames.OutputFileLong,
				HelpText = "The name of the output file. If not specified then the report with analysis results will be outputted to the console window.")]
		public string? OutputFileName { get; }

		/// <summary>
		/// This flag regulates how the locations of API usages will be outputted.<br/>
		///	By default, file paths in locations are relative to the containing project directory.<br/>
		///	However, if this flag is set, then the absolute file paths will be used.
		/// </summary>
		[Option(longName: CommandLineArgNames.OutputAbsolutePathsToUsages,
				HelpText = """
						   This flag regulates how the locations of API usages will be outputted.
						   By default, file paths in locations are relative to the containing project directory.
						   However, if this flag is set, then the absolute file paths will be used.
						   """)]
		public bool OutputAbsolutePathsToUsages { get; }

		/// <summary>
		/// The report output format. There are two supported values:
		/// <list type="bullet">
		/// <item>"text" to output the report in plain text, this is the default output mode,</item>
		/// <item>"json" to output the report in JSON format.</item>
		/// </list>
		/// </summary>
		[Option(longName: CommandLineArgNames.OutputFormat,
				HelpText = """
						   The report output format. There are two supported values:
						   - "text" to output the report in plain text, this is the default output mode,
						   - "json" to output the report in JSON format.
						   """)]
		public string? OutputFormat { get; }

		// Constructor arguments order must be the same as the properties order. This allows command line parser to initialize immutable options object via constructor.
		// See this for details: https://github.com/commandlineparser/commandline/wiki/Immutable-Options-Type
		public CommandLineOptions(string codeSource, string verbosity, bool disableSuppressionMechanism, string? msBuildPath, string? outputFileName, 
								  bool outputAbsolutePathsToUsages, string? outputFormat)
		{
			CodeSource 					= codeSource;
			Verbosity 					= verbosity;
			DisableSuppressionMechanism = disableSuppressionMechanism;
			MSBuildPath 				= msBuildPath;
			OutputFileName				= outputFileName;
			OutputAbsolutePathsToUsages = outputAbsolutePathsToUsages;
			OutputFormat				= outputFormat;
		}
	}
}