using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Output.Grouping;
using Acuminator.Runner.Resources;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Input
{
    internal class AnalysisContextBuilder
	{
		public AnalysisContext CreateContext(CommandLineOptions commandLineOptions)
		{
			commandLineOptions.ThrowOnNull();

			var codeSource = ReadCodeSource(commandLineOptions.CodeSource) ?? 
							 throw new ArgumentException(Messages.CodeSourceNotSpecifiedError);

			var codeAnalysisSettings = new CodeAnalysisSettings(CodeAnalysisSettings.DefaultRecursiveAnalysisEnabled,
																commandLineOptions.IsvSpecificAnalysisIsEnabled,
																staticAnalysisEnabled: true,                   // no sense to run the tool with disabled static analysis
																suppressionMechanismEnabled: !commandLineOptions.DisableSuppressionMechanism,
																commandLineOptions.PX1007DiagnosticIsEnabled);

			var bannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled: !commandLineOptions.DisablePX1099Diagnostic,
														  commandLineOptions.BannedApiFilePath, 
														  commandLineOptions.AllowedApisFilePath);

			OutputFormat outputFormat = GetOutputFormat(commandLineOptions.OutputFormat.NullIfWhiteSpace());
			GroupingMode groupingMode = GetGroupingMode(commandLineOptions.ReportGrouping);
			var input = new AnalysisContext(codeSource, codeAnalysisSettings, bannedApiSettings, commandLineOptions.MSBuildPath, 
											commandLineOptions.OutputFileName, commandLineOptions.OutputAbsolutePathsToUsages, outputFormat,
											commandLineOptions.GenerateSuppressionFile, groupingMode);
			return input;
		}

		private ICodeSource? ReadCodeSource(string codeSourceLocation)
		{
			if (codeSourceLocation.IsNullOrWhiteSpace())
				return null;

			if (!File.Exists(codeSourceLocation))
			{
				string errorMessage = string.Format(CultureInfo.InvariantCulture, Messages.CodeSourceNotFoundError, codeSourceLocation);
				throw new ArgumentException(errorMessage);
			}

			string fullPath = Path.GetFullPath(codeSourceLocation);
			ICodeSource codeSource = CreateCodeSource(fullPath);
			return codeSource;
		}

		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "The message is supposed to be localized with a current culture")]
		private ICodeSource CreateCodeSource(string codeSourceLocation)
		{
			string extension = Path.GetExtension(codeSourceLocation);

			return extension switch
			{
				CommonConstants.ProjectFileExtension  => new ProjectCodeSource(codeSourceLocation),
				CommonConstants.SolutionFileExtension => new SolutionCodeSource(codeSourceLocation),
				_									  => throw new NotSupportedException(
															string.Format(Messages.NotSupportedCodeSourceType, codeSourceLocation))
			};
		}

		private OutputFormat GetOutputFormat(string? rawOutputFormat)
		{
			const string plainTextFormat = "text";
			const string jsonFormat = "json";

			if (rawOutputFormat == null || plainTextFormat.Equals(rawOutputFormat, StringComparison.OrdinalIgnoreCase))
				return OutputFormat.PlainText;
			else if (jsonFormat.Equals(rawOutputFormat, StringComparison.OrdinalIgnoreCase))
				return OutputFormat.Json;
			else
			{
				string errorMsg = string.Format(CultureInfo.CurrentCulture, Messages.NotSupportedOutputFormat, rawOutputFormat, plainTextFormat, jsonFormat);
				throw new NotSupportedException(errorMsg);
			}
		}

		private GroupingMode GetGroupingMode(string? rawGroupingLocation)
		{
			if (rawGroupingLocation.IsNullOrWhiteSpace())
				return GroupingMode.None;

			const char FileGroupingChar = 'F';
			const char DiagnosticGroupingChar = 'D';

			string rawGroupingLocationUppered = rawGroupingLocation.ToUpperInvariant();

			bool groupByFiles = rawGroupingLocationUppered.Contains(FileGroupingChar);
			bool groupByDiagnostic = rawGroupingLocationUppered.Contains(DiagnosticGroupingChar);

			GroupingMode grouping = groupByFiles
				? GroupingMode.Files
				: GroupingMode.None;

			if (groupByDiagnostic)
				grouping |= GroupingMode.Diagnostics;

			return grouping;
		}
	}
}