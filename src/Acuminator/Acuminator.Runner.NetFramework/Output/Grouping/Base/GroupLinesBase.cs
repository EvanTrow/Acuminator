using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Base class to group Acuminator diagnostics.
	/// </summary>
	internal abstract class GroupLinesBase : IGroupLines
	{
		/// <summary>
		/// The diagnostics' grouping mode.
		/// </summary>
		public GroupingMode Grouping { get; }

		protected GroupLinesBase(GroupingMode grouping)
		{
			Grouping = grouping;
		}

		/// <inheritdoc cref="IGroupLines.GetGroupedDiagnostics(AnalysisContext, IEnumerable{Diagnostic}, string?, CancellationToken)"/>
		public abstract IEnumerable<ReportGroup> GetGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> diagnostics,
																	   string? projectDirectory, CancellationToken cancellation);

		protected IEnumerable<Line> GetFlatErrorWithLocationLines(IEnumerable<Diagnostic> unsortedDiagnostics, string? projectDirectory, 
																  AnalysisContext analysisContext)
		{
			var sortedApisWithLocations = unsortedDiagnostics.Select(d => (Diagnostic: d,
																		   Location: GetPrettyLocation(d, projectDirectory, analysisContext)))
															 .OrderBy(diagnosticWithLocation => diagnosticWithLocation.Diagnostic.Id)
															 .ThenBy(diagnosticWithLocation  => diagnosticWithLocation.Location)
															 .Select(diagnosticWithLocation  => new Line(GetDiagnosticContent(diagnosticWithLocation.Diagnostic), 
																										 diagnosticWithLocation.Location));
			return sortedApisWithLocations;
		}

		protected IEnumerable<Line> GetApiUsagesLines(IEnumerable<Diagnostic> sortedDiagnostics, string? projectDirectory,
													  AnalysisContext analysisContext) =>
			sortedDiagnostics.Select(diagnostic => GetDiagnosticLocationLine(diagnostic, projectDirectory, analysisContext));

		protected Line GetDiagnosticLocationLine(Diagnostic diagnostic, string? projectDirectory, AnalysisContext analysisContext)
		{
			var prettyLocation = GetPrettyLocation(diagnostic, projectDirectory, analysisContext);
			return new Line(prettyLocation);
		}

		protected string GetPrettyLocation(Diagnostic diagnostic, string? projectDirectory, AnalysisContext analysisContext)
		{
			string prettyLocation = diagnostic.Location.GetMappedLineSpan().ToString();

			if (analysisContext.OutputAbsolutePathsToUsages || projectDirectory.IsNullOrWhiteSpace())
				return prettyLocation;

			StringComparison stringComparison = analysisContext.CaseSensitiveFilePaths
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			if (!prettyLocation.StartsWith(projectDirectory, stringComparison))
				return prettyLocation;

			string relativeLocation = "." + prettyLocation.Substring(projectDirectory.Length);
			return relativeLocation;
		}

		protected string GetDiagnosticContent(Diagnostic diagnostic)
		{
			string errorMessage = diagnostic.GetMessage(CultureInfo.CurrentCulture).NullIfWhiteSpace() ??
								  diagnostic.GetMessage(CultureInfo.InvariantCulture).NullIfWhiteSpace() ??
								  diagnostic.Descriptor.Title.ToString(CultureInfo.InvariantCulture);

			return $"{diagnostic.Id}: {errorMessage}";
		}
	}
}
