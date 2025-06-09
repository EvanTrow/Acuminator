using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Results grouper that either groups found diagnostics by ID or does not apply grouping at all.
	/// </summary>
	internal class GroupByDiagnosticsOrNoGrouping : GroupLinesBase
	{
		public GroupByDiagnosticsOrNoGrouping(GroupingMode grouping) : base(grouping)
		{ }

		/// <summary>
		/// Group reported diagnostics by <see cref="Diagnostic.Id"/> or do not apply grouping at all.
		/// </summary>
		/// <param name="analysisContext">Analysis context.</param>
		/// <param name="diagnostics">Diagnostics to group.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// Acuminator diagnostics grouped by <see cref="Diagnostic.Id"/> or not grouped at all.
		/// </returns>
		public override IEnumerable<ReportGroup> GetGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> diagnostics,
																		string? projectDirectory, CancellationToken cancellation) =>
			GroupDiagnosticsByDiagnosticIdOrNothing(analysisContext, diagnostics, projectDirectory, cancellation);

		protected IReadOnlyCollection<ReportGroup> GroupDiagnosticsByDiagnosticIdOrNothing(AnalysisContext analysisContext, 
																						   IEnumerable<Diagnostic> unsortedDiagnostics,
																						   string? projectDirectory, CancellationToken cancellation)
		{
			return analysisContext.GroupingMode.HasGrouping(GroupingMode.DiagnosticIDs)
				? GroupDiagnosticsByDiagnosticId(unsortedDiagnostics, projectDirectory, analysisContext, cancellation).ToList()
				: GetNotGroupedDiagnostics(analysisContext, unsortedDiagnostics, projectDirectory, cancellation);
		}

		private IEnumerable<ReportGroup> GroupDiagnosticsByDiagnosticId(IEnumerable<Diagnostic> unsortedDiagnostics, string? projectDirectory,
																		AnalysisContext analysisContext, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var diagnosticsGroupedById = unsortedDiagnostics.GroupBy(d => d.Id)
															.OrderBy(d => d.Key);

			foreach (var diagnosticsWithSameId in diagnosticsGroupedById)
			{
				cancellation.ThrowIfCancellationRequested();

				string diagnosticId	= diagnosticsWithSameId.Key;
				var diagnosticsWithLocations = GetOrderedDiagnosticsWithLocationLines(diagnosticsWithSameId, projectDirectory, analysisContext, 
																					  sortBySourceFile: true, sortByDiagnosticId: false)
																		.ToList();
				var diagnosticIdGroup = new ReportGroup
				{
					GroupTitle 			  = new Title(diagnosticId, TitleKind.DiagnosticId),
					TotalDiagnosticsCount = diagnosticsWithLocations.Count,
					Lines 				  = diagnosticsWithLocations.NullIfEmpty()
				};

				yield return diagnosticIdGroup;
			}
		}

		private IReadOnlyCollection<ReportGroup> GetNotGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> unsortedDiagnostics,
																		  string? projectDirectory, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			IReadOnlyCollection<Diagnostic> diagnosticsMaterializedCollection = (unsortedDiagnostics as IReadOnlyCollection<Diagnostic>) ?? 
																				 unsortedDiagnostics.ToList();
			var distinctDiagnosticsCount = diagnosticsMaterializedCollection.GroupBy(d => d.Id)
																			.Count();
			var sortedNotGroupedDiagnostics = GetOrderedDiagnosticsWithLocationLines(diagnosticsMaterializedCollection, projectDirectory, analysisContext,
																		   sortBySourceFile: true, sortByDiagnosticId: true)
												.ToList(capacity: diagnosticsMaterializedCollection.Count);

			cancellation.ThrowIfCancellationRequested();

			var singleGroupWithAllDiagnosticsOrdered = new ReportGroup
			{
				TotalDiagnosticsCount	 = diagnosticsMaterializedCollection.Count,
				DistinctDiagnosticsCount = distinctDiagnosticsCount,
				Lines					 = sortedNotGroupedDiagnostics.NullIfEmpty()
			};

			return [singleGroupWithAllDiagnosticsOrdered];
		}
	}
}
