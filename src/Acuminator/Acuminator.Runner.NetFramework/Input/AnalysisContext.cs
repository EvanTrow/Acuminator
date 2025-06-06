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

		public CodeAnalysisSettings CodeAnalysisSettings { get; }

		public BannedApiSettings BannedApiSettings { get; }

		/// <inheritdoc cref="CommandLineOptions.MSBuildPath"/>
		public string? MSBuildPath { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFileName"/>
		public string? OutputFileName { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputAbsolutePathsToUsages"/>
		public bool OutputAbsolutePathsToUsages { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFormat"/>
		public OutputFormat OutputFormat { get; }

		/// <inheritdoc cref="CommandLineOptions.GenerateSuppressionFile"/>
		public bool GenerateSuppressionFile { get; }

		/// <summary>
		/// The grouping mode for Acuminator errors in the report.
		/// </summary>
		public GroupingMode GroupingMode { get; }

		public AnalysisContext(ICodeSource codeSource, CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings, 
							   string? msBuildPath, string? outputFileName, bool outputAbsolutePathsToUsages, OutputFormat outputFormat,
							   bool generateSuppressionFile, GroupingMode groupingMode)
		{
			CodeSource 					= codeSource.CheckIfNull();
			CodeAnalysisSettings		= codeAnalysisSettings.CheckIfNull();
			BannedApiSettings			= bannedApiSettings.CheckIfNull();
			MSBuildPath 				= msBuildPath.NullIfWhiteSpace();
			OutputFileName				= outputFileName.NullIfWhiteSpace();
			OutputAbsolutePathsToUsages = outputAbsolutePathsToUsages;
			OutputFormat				= outputFormat;
			GenerateSuppressionFile		= generateSuppressionFile;
			GroupingMode				= groupingMode;
		}
	}
}