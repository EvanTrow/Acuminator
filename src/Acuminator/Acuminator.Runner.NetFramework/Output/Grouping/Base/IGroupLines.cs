using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

using Acuminator.Runner.Input;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output
{
	/// <summary>
	/// Interface for helpers that group report lines.
	/// </summary>
	internal interface IGroupLines
	{
		/// <summary>
		/// Gets the required output results grouping.
		/// </summary>
		GroupingMode Grouping { get; }

		/// <summary>
		/// Get API groups
		/// </summary>
		/// <param name="analysisContext">Analysis context.</param>
		/// <param name="diagnostics">Diagnostics to group.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// Output API results grouped by <see cref="Grouping"/>.
		/// </returns>
		IEnumerable<ReportGroup> GetApiGroups(AnalysisContext analysisContext, ImmutableArray<Diagnostic> diagnostics,
											  string? projectDirectory, CancellationToken cancellation);
	}
}
