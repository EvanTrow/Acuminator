using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Runner.Input;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

using Serilog;

using DiagnosticAnalyzer = Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer;
using AnalyzerFileReference = Microsoft.CodeAnalysis.Diagnostics.AnalyzerFileReference;
using Acuminator.Utilities.DiagnosticSuppression;
using System.Linq;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Resources;

namespace Acuminator.Runner.Analysis.Initialization
{
	internal class AcuminatorAnalysisInitializer
	{
		private readonly AnalysisContext _analysisContext;
		private readonly ILogger _logger;

		public AcuminatorAnalysisInitializer(AnalysisContext analysisContext, ILogger logger)
		{
			_analysisContext = analysisContext.CheckIfNull();
			_logger = logger.CheckIfNull();
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		public (bool AreSettingsInitialized, ImmutableArray<DiagnosticAnalyzer> Analyzers) InitializeAcuminatorSettingsAndGetAnalyzers()
		{
			try
			{
				GlobalSettings.InitializeGlobalSettingsOnce(_analysisContext.CodeAnalysisSettings, _analysisContext.BannedApiSettings);

				var analyzers = CollectAnalyzers();

				var acuminatorVersion = typeof(Acuminator.SharedConstants).Assembly.GetName()?.Version;

				if (acuminatorVersion != null)
					_logger.Information(Messages.AcuminatorVersionInfo, acuminatorVersion);
				else
					_logger.Warning(Messages.FailedToObtainAcuminatorVersionWarning);

				return (AreSettingsInitialized: true, Analyzers: analyzers);
			}
			catch (Exception e)
			{
				_logger.Error(e, Messages.ErrorDuringAcuminatorAnalyzersCollection);
				return (AreSettingsInitialized: false, Analyzers: []);
			}
		}

		private ImmutableArray<DiagnosticAnalyzer> CollectAnalyzers()
		{
			var acuminatorAnalyzersPath = typeof(PXDiagnosticAnalyzer).Assembly.Location;
			var analyzerReference = new AnalyzerFileReference(acuminatorAnalyzersPath, new AnalyzerAssemblyLoader());
			var analyzers = analyzerReference.GetAnalyzers(LanguageNames.CSharp);

			return analyzers;
		}

		public bool InitializeAcuminatorGlobalSuppressionMechanismForCodeSource(Solution solution)
		{
			solution.ThrowOnNull();

			switch (_analysisContext.CodeSource.Type)
			{
				case CodeSources.CodeSourceType.Project:
					var project = _analysisContext.CodeSource.GetProjectsForValidation(solution).FirstOrDefault();

					return project != null
						? InitializeAcuminatorGlobalSuppressionMechanismForProject(project)
						: true;

				case CodeSources.CodeSourceType.Solution:
					return InitializeAcuminatorGlobalSuppressionMechanismForSolution(solution);

				default:
					if (_analysisContext.GenerateSuppressionFile)
					{
						_logger.Error("""
									  The Acuminator console tool is configured to generate a suppression file for the code source "{CodeSource}" but the code source is not a project or solution.
									  The generation of Acuminator suppression file is supported only for a project or solution.
									  """,
									 _analysisContext.CodeSource.Location);
						return false;
					}
					else
					{
						_logger.Warning("Acuminator suppression via suppression file is not supported for the code source \"{CodeSource}\". The code source is not a project or solution.",
										_analysisContext.CodeSource.Location);
						return true;
					}
			}
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use a string variable due to a long message")]
		private bool InitializeAcuminatorGlobalSuppressionMechanismForProject(Project project)
		{
			var acuminatorSuppressionFiles = project.AdditionalDocuments
													.Where(d => IsAcuminatorSuppressionFile(d.FilePath))
													.Select(d => new SuppressionManagerInitInfo(d.FilePath!, _analysisContext.GenerateSuppressionFile))
													.ToList(capacity: 1);

			if (_analysisContext.GenerateSuppressionFile && acuminatorSuppressionFiles.Count == 0)
			{
				string errorMsg = CreateErrorMessage();
				_logger.Error(errorMsg);
				return false;
			}

			SuppressionManager.InitOrReset(acuminatorSuppressionFiles,
										   fileSystemServiceFabric: () => new SuppressionFileSystemServiceForConsoleRunner(
																				new ConsoleRunnerIOErrorObserver(_logger)));
			return true;

			//-----------------------------------------Local Function-----------------------------------------------------
			string CreateErrorMessage()
			{
				string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
				return $"""
					   Acuminator console tool is configured to generate a suppression file for the project "{project.Name}" 
					   because --{CommandLineArgNames.GenerateSuppressionFileLong} flag was passed among command line arguments.
					   However, the project does not contain an Acuminator suppression file.
					   To generate the project's suppression file you need to create an empty suppression file first with the name "{suppressionFileName}" in the project's root folder.
					   The content of the empty suppression file should be like this:

					   <?xml version="1.0" encoding="utf-8"?>
					   <suppressions>
					   </suppressions>

					   You must also add it to the "AdditionalFiles" section in the project file "{project.FilePath}" like this:

					   <ItemGroup>
					     <AdditionalFiles Include="{suppressionFileName}"/>
					   </ItemGroup>
					   """;
			}
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use a string variable due to a long message")]
		private bool InitializeAcuminatorGlobalSuppressionMechanismForSolution(Solution solution)
		{
			var acuminatorSuppressionFiles = solution.Projects
													 .SelectMany(project => project.AdditionalDocuments)
													 .Where(d => IsAcuminatorSuppressionFile(d.FilePath))
													 .Select(d => new SuppressionManagerInitInfo(d.FilePath!, _analysisContext.GenerateSuppressionFile))
													 .ToList(capacity: 1);

			if (_analysisContext.GenerateSuppressionFile && acuminatorSuppressionFiles.Count == 0)
			{
				string errorMsg = CreateErrorMessage();
				_logger.Error(errorMsg);
				return false;
			}

			SuppressionManager.InitOrReset(acuminatorSuppressionFiles,
										   fileSystemServiceFabric: () => new SuppressionFileSystemServiceForConsoleRunner(
																				new ConsoleRunnerIOErrorObserver(_logger)));
			return true;

			//-----------------------------------------Local Function-----------------------------------------------------
			string CreateErrorMessage()
			{
				return $"""
					   Acuminator console tool is configured to generate a suppression file for projects of the solution "{solution.FilePath}" 
					   because --{CommandLineArgNames.GenerateSuppressionFileLong} flag was passed among command line arguments.
					   However, the solution does not contain any project with an Acuminator suppression file.
					   To generate a suppression file for a project you need to create an empty suppression file for the project first with the name "<ProjectName>.{SuppressionFile.SuppressionFileExtension}" in the project's root folder:
					   The content of the empty suppression file should be like this:

					   <?xml version="1.0" encoding="utf-8"?>
					   <suppressions>
					   </suppressions>

					   You must also add it to the "AdditionalFiles" section in the project .csproj file like this:

					   <ItemGroup>
					     <AdditionalFiles Include="<ProjectName>.{SuppressionFile.SuppressionFileExtension}"/>
					   </ItemGroup>
					   """;
			}
		}

		private bool IsAcuminatorSuppressionFile([NotNullWhen(returnValue: true)]string? filePath)
		{
			if (filePath.IsNullOrWhiteSpace() || !File.Exists(filePath))
				return false;

			string extension = Path.GetExtension(filePath);
			return SuppressionFile.SuppressionFileExtension.Equals(extension, StringComparison.Ordinal);
		}
	}
}
