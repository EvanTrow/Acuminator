using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Output;
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
			var input = new AnalysisContext(codeSource, codeAnalysisSettings, bannedApiSettings, commandLineOptions.MSBuildPath, 
											commandLineOptions.OutputFileName, commandLineOptions.OutputAbsolutePathsToUsages, outputFormat);
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

		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "The message is supposed to be localized with a current culture")]
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
				string errorMsg = string.Format(Messages.NotSupportedOutputFormat, rawOutputFormat, plainTextFormat, jsonFormat);
				throw new NotSupportedException(errorMsg);
			}
		}
	}
}