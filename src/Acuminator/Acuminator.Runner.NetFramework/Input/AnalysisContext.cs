using System;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Output;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Input
{
    internal class AnalysisContext
	{
		/// <summary>
		/// Gets the code source to validate.
		/// </summary>
		/// <value>
		/// The code source to validate.
		/// </value>
		public ICodeSource CodeSource { get; }

		/// <inheritdoc cref="CommandLineOptions.MSBuildPath"/>
		public string? MSBuildPath { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFileName"/>
		public string? OutputFileName { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputAbsolutePathsToUsages"/>
		public bool OutputAbsolutePathsToUsages { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFormat"/>
		public OutputFormat OutputFormat { get; }

		public CodeAnalysisSettings CodeAnalysisSettings { get; }

		public BannedApiSettings BannedApiSettings { get; }

		public AnalysisContext(ICodeSource codeSource, CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings, 
							   string? msBuildPath, string? outputFileName, bool outputAbsolutePathsToUsages, OutputFormat outputFormat)
		{
			CodeSource 					= codeSource.CheckIfNull();
			CodeAnalysisSettings		= codeAnalysisSettings.CheckIfNull();
			BannedApiSettings			= bannedApiSettings.CheckIfNull();
			MSBuildPath 				= msBuildPath.NullIfWhiteSpace();
			OutputFileName				= outputFileName.NullIfWhiteSpace();
			OutputAbsolutePathsToUsages = outputAbsolutePathsToUsages;
			OutputFormat				= outputFormat;
		}
	}
}