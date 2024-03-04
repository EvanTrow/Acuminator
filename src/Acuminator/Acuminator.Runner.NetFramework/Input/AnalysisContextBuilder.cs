using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Output;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Input
{
    internal class AnalysisContextBuilder
	{
		public AppAnalysisContext CreateContext(CommandLineOptions commandLineOptions)
		{
			commandLineOptions.CheckIfNull(nameof(commandLineOptions));

			var codeSource = ReadCodeSource(commandLineOptions.CodeSource) ?? throw new ArgumentException(Messages.CodeSourceNotSpecifiedError);
			OutputFormat outputFormat = GetOutputFormat(commandLineOptions.OutputFormat.NullIfWhiteSpace());
			var input = new AppAnalysisContext(codeSource, commandLineOptions.DisableSuppressionMechanism, commandLineOptions.MSBuildPath, 
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

		private ICodeSource CreateCodeSource(string codeSourceLocation)
		{
			string extension = Path.GetExtension(codeSourceLocation);

			return extension switch
			{
				CommonConstants.ProjectFileExtension  => new ProjectCodeSource(codeSourceLocation),
				CommonConstants.SolutionFileExtension => new SolutionCodeSource(codeSourceLocation),
				_									  => throw new NotSupportedException(
															string.Format(CultureInfo.InvariantCulture, 
																		  Messages.NotSupportedCodeSourceType, codeSourceLocation))
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
				string errorMsg = string.Format(CultureInfo.InvariantCulture, Messages.NotSupportedOutputFormat, rawOutputFormat, 
												plainTextFormat, jsonFormat);
				throw new NotSupportedException(errorMsg);
			}
		}
	}
}