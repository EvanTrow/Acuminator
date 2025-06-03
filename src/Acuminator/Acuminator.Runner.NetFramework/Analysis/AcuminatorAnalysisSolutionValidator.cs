using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Runner.Analysis.Helpers;
using Acuminator.Runner.Input;
using Acuminator.Runner.Output;

using Acuminator.Utilities.Common;
using Acuminator.Analyzers.StaticAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Serilog;

namespace Acuminator.Runner.Analysis
{
    internal sealed class AcuminatorAnalysisSolutionValidator
	{
		private readonly IOutputterFactory _outputterFactory;
		private readonly IProjectReportBuilder	_reportBuilder;

		private readonly ImmutableArray<DiagnosticAnalyzer> _diagnosticAnalyzers;

		private AcuminatorAnalysisSolutionValidator(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers,
													IProjectReportBuilder? customReportBuilder = null, 
													IOutputterFactory? customOutputFactory = null)
		{ 
			_diagnosticAnalyzers = diagnosticAnalyzers;
			_reportBuilder 		 = customReportBuilder ?? new ProjectReportBuilder();
			_outputterFactory 	 = customOutputFactory ?? new ReportOutputterFactory();
		}

		public static AcuminatorAnalysisSolutionValidator CreateAcuminatorSolutionAnalyzer()
		{
			var acuminatorAnalysisInitializer = new AcuminatorAnalysisInitializer(analysisContext, logger);
			var (areSettingsInitialized, diagnosticAnalyzers) = acuminatorAnalysisInitializer.InitializeAcuminatorSettingsAndGetAnalyzers();

			return areSettingsInitialized && !diagnosticAnalyzers.IsDefaultOrEmpty
					? new AcuminatorAnalysisSolutionValidator(diagnosticAnalyzers, logger, acuminatorAnalysisInitializer)
					: null;
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		public async Task<RunResult> AnalyseSolution(Solution solution, Input.AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			if (_diagnosticAnalyzers.IsDefaultOrEmpty)
			{
				_logger.Error(Messages.FailedToLoadAcuminatorAnalyzersError, analysisContext.CodeSource.Location);
				return RunResult.RunTimeError;
		}

			if (!_acuminatorAnalysisInitializer.InitializeAcuminatorGlobalSuppressionMechanismForCodeSource(solution))
		{
				_logger.Error(Messages.FailedToInitializeAcuminatorGlobalSuppressionMechanismError, analysisContext.CodeSource.Location);
				return RunResult.RunTimeError;
			}

			RunResult solutionValidationResult = RunResult.Success;
			var projectsToValidate = analysisContext.CodeSource.GetProjectsForValidation(solution)
															   .OrderBy(p => p.Name);

			using (var reportOutputter = _outputterFactory.CreateOutputter(analysisContext))
			{
				var projectReports = new List<ProjectReport>(capacity: solution.ProjectIds.Count);
				bool hasProjectReferencingAcumatica = false;

				foreach (Project project in projectsToValidate)
				{
					_logger.Information(Messages.StartedAcuminatorValidationOfTheProjectInfo, project.Name);

					if (cancellationToken.IsCancellationRequested)
					{
						_logger.Information(Messages.CancelledCodeSourceValidationInfo, analysisContext.CodeSource.Location, project.Name, RunResult.Cancelled);
						solutionValidationResult = solutionValidationResult.Combine(RunResult.Cancelled);
						return solutionValidationResult;
					}

					var (projectValidationResult, projectReport, isPlatformReferenced) =
						await AnalyzeProject(project, analysisContext, cancellationToken).ConfigureAwait(false);

					hasProjectReferencingAcumatica = hasProjectReferencingAcumatica || isPlatformReferenced;

					if (projectReport != null)
						projectReports.Add(projectReport);

					solutionValidationResult = solutionValidationResult.Combine(projectValidationResult);

					_logger.Information(Messages.FinishedAcuminatorValidationOfTheProjectInfo, project.Name, projectValidationResult);
					}

				if (!hasProjectReferencingAcumatica)
				{
					_logger.Error(Messages.NoProjectInCodeSourceReferencesAcumaticaPlatformError, analysisContext.CodeSource.Location);
					return RunResult.RunTimeError;
				}

				if (projectReports.Count > 0)
				{
					var codeSourceReport = new CodeSourceReport(analysisContext.CodeSource.Location, projectReports);
					reportOutputter.OutputReport(codeSourceReport, analysisContext, cancellationToken);
				}
			}

			return solutionValidationResult;
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		private async Task<(RunResult ValidationResult, ProjectReport? Report, bool IsPlatformReferenced)> AnalyzeProject(Project project, 
																												Input.AnalysisContext analysisContext, 
																								CancellationToken cancellationToken)
		{
			_logger.Debug(Messages.ObtainingRoslynCompilationDataForTheProjectDebug, project.Name);
			var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

			if (compilation == null)
			{
				_logger.Error(Messages.FailedToObtainRoslynCompilationDataForTheProjectError, project.Name, project.FilePath);
				return (RunResult.RunTimeError, Report: null, IsPlatformReferenced: false);
			}

			_logger.Debug(Messages.ObtainedRoslynCompilationDataForTheProjectDebug, project.Name);

			if (!IsPlatformReferenced(compilation))
			{
				if (analysisContext.CodeSource.Type == CodeSources.CodeSourceType.Project)
				{
					_logger.Error(Messages.ProjectDoesNotReferenceAcumaticaPlatformValidationError, project.Name);
					return (RunResult.RequirementsNotMet, Report: null, IsPlatformReferenced: false);
		}
				else
			{
					// For solution with multiple projects we will not fail only if there are no projects referencing Acumatica Platform.
					_logger.Warning(Messages.ProjectDoesNotReferenceAcumaticaPlatformValidationError, project.Name);
					return (RunResult.Success, Report: null, IsPlatformReferenced: false);
				}
			}

			var (validationResult, projectReport) = await RunAnalyzersOnProjectAsync(compilation, analysisContext, project, cancellationToken)
															.ConfigureAwait(false);
			return (validationResult, projectReport, IsPlatformReferenced: true);
		}

		private bool IsPlatformReferenced(Compilation compilation)
			{
			var acuminatorPxContext = new PXContext(compilation, null);
			return acuminatorPxContext.IsPlatformReferenced;
			}


		private async Task<(RunResult validationResult, ProjectReport? Report, DiagnosticsWithBannedApis? AnalysisData)> RunAnalyzersOnProjectAsync(
																						Compilation compilation, Input.AnalysisContext analysisContext, 
																						Project project, CancellationToken cancellation)
		{
			if (_diagnosticAnalyzers.IsDefaultOrEmpty)
				return (RunResult.Success, Report: null, AnalysisData: null);

			SuppressionManager.UseSuppression = !analysisContext.DisableSuppressionMechanism;
			var compilationAnalysisOptions = new CompilationWithAnalyzersOptions(options: null!, OnAnalyzerException,
																				 concurrentAnalysis: !Debugger.IsAttached, 
																				 logAnalyzerExecutionTime: false);
			CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(compilationAnalysisOptions, _diagnosticAnalyzers, cancellation);

			var diagnosticResults = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellation).ConfigureAwait(false);
			Log.Error("{Project} - Total Errors Count: {ErrorCount}", project.Name, diagnosticResults.Length);

			if (diagnosticResults.IsDefaultOrEmpty)
				return (RunResult.Success, Report: null, AnalysisData: null);

			var diagnosticsWithApis		= new DiagnosticsWithBannedApis(diagnosticResults, analysisContext);
			ProjectReport projectReport = _reportBuilder.BuildReport(diagnosticsWithApis, analysisContext, project, cancellation);

			return (RunResult.RequirementsNotMet, projectReport, diagnosticsWithApis);
		}

		private CodeSourceReport CreateCodeSourceReport(Input.AnalysisContext analysisContext, List<ProjectReport> projectReports, IEnumerable<Api> allDistinctApis)
		{
			if (analysisContext.IncludeAllDistinctApis) 
			{
				var sortedDistinctApiLines = allDistinctApis.OrderBy(api => api.FullName)
															.Select(api => new Line(api.FullName))
															.ToList();

				var codeSourceReport = new CodeSourceReport(analysisContext.CodeSource.Location, sortedDistinctApiLines, projectReports);
				return codeSourceReport;
			}
			else
			{
				int allDistinctApisCount = allDistinctApis.Count();
				var codeSourceReport = new CodeSourceReport(analysisContext.CodeSource.Location, allDistinctApisCount, projectReports);
				return codeSourceReport;
			}
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use runtime dependent new line in message")]
		private void OnAnalyzerException(Exception exception, DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
		{
			var prettyLocation = diagnostic.Location.GetMappedLineSpan().ToString();

			string errorMsg = $"Analyzer error:{Environment.NewLine}{{Id}}{Environment.NewLine}{{Location}}{Environment.NewLine}{{Analyzer}}";
			Log.Error(exception, errorMsg, diagnostic.Id, prettyLocation, analyzer.ToString());
		}
	}
}