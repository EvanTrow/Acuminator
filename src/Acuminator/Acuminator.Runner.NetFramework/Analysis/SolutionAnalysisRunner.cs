using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using Serilog;

using Acuminator.Runner.Input;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Analysis
{
	/// <summary>
	/// A solution analysis runner that does preparatory work - register MSBuild, load solution for analysis and calls analyzer.
	/// </summary>
	[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Resource strings are used for logged messages")]
	internal class SolutionAnalysisRunner
	{
		public async Task<RunResult> RunAnalysisAsync(AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			analysisContext.ThrowOnNull(nameof(analysisContext));

			if (cancellationToken.IsCancellationRequested)
				return RunResult.Cancelled;
			else if (!TryRegisterMSBuild(analysisContext))
				return RunResult.RunTimeError;

			RunResult runResult = RunResult.Success;
			bool hasErrors = false;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				runResult = await LoadAndAnalyzeCodeSourceAsync(analysisContext, cancellationToken);
			}
			catch (OperationCanceledException cancellationException)
			{
				Log.Warning(cancellationException, Messages.CodeSourceValidationWasCancelled,
							analysisContext.CodeSource.Location);
				runResult = RunResult.Cancelled;
			}
			catch (Exception exception)
			{
				Log.Error(exception, Messages.AnalysisOfCodeSourceRuntimeError, analysisContext.CodeSource.Location);
				hasErrors = true;
			}
			finally
			{
				if (!TryUnregisterMSBuild())
					hasErrors = true;
			}

			return hasErrors
				? RunResult.RunTimeError
				: runResult;
		}

		private async Task<RunResult> LoadAndAnalyzeCodeSourceAsync(AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			Log.Information(Messages.StartAnalyzingTheCodeSourceStatusMessage, analysisContext.CodeSource.Location);

			using var workspace = MSBuildWorkspace.Create();

			try
			{
				workspace.WorkspaceFailed += OnCodeSourceLoadError;

				Log.Information(Messages.StartLoadingTheCodeSourceAtPathStatusMessage, analysisContext.CodeSource.Location);
				var solution = await analysisContext.CodeSource.LoadSolutionAsync(workspace, cancellationToken)
															   .ConfigureAwait(false);
				if (solution == null)
				{
					Log.Error(Messages.FailedToLoadSolutionFromCodeSourceError, analysisContext.CodeSource.Location);
					return RunResult.RunTimeError;
				}

				Log.Information(Messages.SuccessfullyLoadedCodeSourceAtPathStatusMessage, analysisContext.CodeSource.Location);
				Log.Debug(Messages.LoadedProjectsCount_Information, solution.ProjectIds.Count);

				Log.Information(Messages.InitializeAcuminatorAnalyzersStatusMessage);
				var solutionCompatibilityAnalyzer = AcuminatorAnalysisSolutionValidator.CreateAcuminatorSolutionAnalyzer();
				Log.Information(Messages.StartValidatingSolutionStatusMessage);

				RunResult validationResult = await solutionCompatibilityAnalyzer.AnalyseSolution(solution, analysisContext, cancellationToken);
				
				Log.Information(Messages.SuccessfullyFinishedSolutionValidationStatusMessage);
				return validationResult;
			}
			finally
			{
				workspace.WorkspaceFailed -= OnCodeSourceLoadError;
			}
		}

		private void OnCodeSourceLoadError(object sender, WorkspaceDiagnosticEventArgs e)
		{
			switch (e.Diagnostic.Kind)
			{
				case WorkspaceDiagnosticKind.Failure:
					Log.Error("{WorkspaceDiagnostic}", e.Diagnostic);
					break;
				case WorkspaceDiagnosticKind.Warning:
					Log.Warning("{WorkspaceDiagnostic}", e.Diagnostic);
					break;
			}
		}

		private bool TryRegisterMSBuild(AnalysisContext analysisContext)
		{
			if (analysisContext.MSBuildPath != null)
			{
				return TryRegisterMSBuildByPath(analysisContext.MSBuildPath);
			}

			Log.Information(Messages.SearchingForMSBuildInstancesStatusMessage);

			var vsInstances = MSBuildLocator.QueryVisualStudioInstances();
			VisualStudioInstance? latestVSInstance = vsInstances.OrderByDescending(vsInstance => vsInstance.Version)
																.FirstOrDefault();
			if (latestVSInstance == null)
			{
				Log.Error(Messages.NoInstalledMSBuildFoundError);
				return false;
			}

			Log.Information(Messages.MSBuild_VisualStudioNameAndVersion_Info, latestVSInstance.Name, latestVSInstance.Version);
			Log.Information(Messages.MSBuildPath_Info, latestVSInstance.MSBuildPath);

			try
			{
				MSBuildLocator.RegisterInstance(latestVSInstance);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, Messages.MSBuildInstanceRegistrationError);
				return false;
			}
		}

		private bool TryRegisterMSBuildByPath(string msBuildPath)
		{
			try
			{
				Log.Information(Messages.RegisteringMSBuildAtTheProvidedPathStatusMessage, msBuildPath);

				string? msBuildDir = Path.GetDirectoryName(msBuildPath);
				MSBuildLocator.RegisterMSBuildPath(msBuildDir);

				Log.Information(Messages.SuccessfullyRegisteredMSBuildAtProvidedPathStatusMessage, msBuildPath);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, Messages.MSBuildRegistrationAtProvidedPathFailedError, msBuildPath);
				return false;
			}
		}

		private bool TryUnregisterMSBuild()
		{
			try
			{
				MSBuildLocator.Unregister();
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, Messages.UnregisterMSBuildInstanceError);
				return false;
			}
		}
	}
}