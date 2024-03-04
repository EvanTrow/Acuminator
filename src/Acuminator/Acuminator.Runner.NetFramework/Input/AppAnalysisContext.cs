using System;
using System.Runtime.InteropServices;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Output;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Input
{
    internal class AppAnalysisContext
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

		/// <inheritdoc cref="CommandLineOptions.DisableSuppressionMechanism"/>
		public bool DisableSuppressionMechanism { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFileName"/>
		public string? OutputFileName { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputAbsolutePathsToUsages"/>
		public bool OutputAbsolutePathsToUsages { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFormat"/>
		public OutputFormat OutputFormat { get; }

		/// <summary>
		/// If true then the unnderlying OS is Linux.
		/// </summary>
		public bool IsRunningOnLinux { get; }

		public AppAnalysisContext(ICodeSource codeSource, bool disableSuppressionMechanism, string? msBuildPath, string? outputFileName, 
								  bool outputAbsolutePathsToUsages, OutputFormat outputFormat)
		{
			CodeSource 					= codeSource.CheckIfNull(nameof(codeSource));
			DisableSuppressionMechanism = disableSuppressionMechanism;
			MSBuildPath 				= msBuildPath.NullIfWhiteSpace();
			OutputFileName				= outputFileName.NullIfWhiteSpace();
			OutputAbsolutePathsToUsages = outputAbsolutePathsToUsages;
			OutputFormat				= outputFormat;
			IsRunningOnLinux			= RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}
	}
}