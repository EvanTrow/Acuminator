using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Utilities;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

using DiagnosticInfo = (Microsoft.CodeAnalysis.Diagnostic Diagnostic, string Content, string Location);

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

		/// <summary>
		/// Gets the ordered diagnostics with location report lines for given <paramref name="unsortedDiagnostics"/>.<br/>
		/// If <paramref name="sortBySourceFile"/> is true, the diagnostics are sorted additionally by source file. This sorting is always applied first.<br/>
		/// If <paramref name="sortByDiagnosticId"/> is true, the diagnostics are sorted additionally by diagnostic identifier. This sorting is applied after sorting by source file.<br/>
		/// </summary>
		/// <param name="unsortedDiagnostics">The unsorted diagnostics.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="analysisContext">Context for the analysis.</param>
		/// <param name="sortBySourceFile">True to sort by source file. This sorting is always applied first.</param>
		/// <param name="sortByDiagnosticId">True to sort by diagnostic identifier. This sorting is applied after sorting by source file.</param>
		/// <returns>
		/// The ordered diagnostics with location report lines for given <paramref name="unsortedDiagnostics"/>.
		/// </returns>
		protected IEnumerable<Line> GetOrderedDiagnosticsWithLocationLines(IEnumerable<Diagnostic> unsortedDiagnostics, string? projectDirectory, 
																		   AnalysisContext analysisContext, bool sortBySourceFile, bool sortByDiagnosticId)
		{
			var unsortedDiagnosticInfos = unsortedDiagnostics.Select(d => (Diagnostic: d,
																		   Content: GetDiagnosticContent(d),
																		   Location: GetPrettyLocation(d, projectDirectory, analysisContext)));

			var sortedDiagnosticInfos = GetSortedDiagnosticInfos(sortByDiagnosticId, sortBySourceFile, unsortedDiagnosticInfos);
			var reportLines	= sortedDiagnosticInfos.Select(d  => new Line(d.Content, d.Location));

			return reportLines;
		}

		private IEnumerable<DiagnosticInfo> GetSortedDiagnosticInfos(bool sortBySourceFile, bool sortByDiagnosticId, 
																	 IEnumerable<DiagnosticInfo> unsortedDiagnosticInfos)
		{
			IOrderedEnumerable<DiagnosticInfo>? sortedDiagnosticInfos = null;

			if (sortBySourceFile)
				sortedDiagnosticInfos = unsortedDiagnosticInfos.OrderBy(d => d.Diagnostic.Location.SourceTree?.FilePath ?? string.Empty);
			
			if (sortByDiagnosticId)
			{
				sortedDiagnosticInfos = sortedDiagnosticInfos?.ThenBy(d => d.Diagnostic.Id) ?? 
										unsortedDiagnosticInfos.OrderBy(d => d.Diagnostic.Id);
			}

			sortedDiagnosticInfos = sortedDiagnosticInfos?.ThenBy(d => d.Location) ??
									unsortedDiagnosticInfos.OrderBy(d => d.Location);
			sortedDiagnosticInfos = sortedDiagnosticInfos.ThenBy(d => d.Content, StringComparer.Ordinal);
			return sortedDiagnosticInfos;
		}

		protected string GetPrettyLocation(Diagnostic diagnostic, string? projectDirectory, AnalysisContext analysisContext)
		{
			string prettyLocation = diagnostic.Location.GetMappedLineSpan().ToString();

			if (analysisContext.OutputAbsolutePathsToUsages || projectDirectory.IsNullOrWhiteSpace())
				return prettyLocation.Enquote();

			StringComparison stringComparison = analysisContext.CaseSensitiveFilePaths
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			if (!prettyLocation.StartsWith(projectDirectory, stringComparison))
				return prettyLocation.Enquote();

			string relativeLocation = "." + prettyLocation.Substring(projectDirectory.Length);
			return relativeLocation.Enquote();
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
