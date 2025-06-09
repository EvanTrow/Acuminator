using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Interface for helpers that group report lines.
	/// </summary>
	internal interface IGroupLines
	{
		/// <summary>
		/// The grouping mode of the grouper.
		/// </summary>
		GroupingMode Grouping { get; }

		/// <summary>
		/// Get Acuminator errors grouped according to the <see cref="Grouping"/> mode.
		/// </summary>
		/// <param name="analysisContext">Analysis context.</param>
		/// <param name="diagnostics">Diagnostics to group.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// Acuminator errors grouped according to the <see cref="Grouping"/> mode.
		/// </returns>
		IEnumerable<ReportGroup> GetGroupedErrors(AnalysisContext analysisContext, ImmutableArray<Diagnostic> diagnostics,
												  string? projectDirectory, CancellationToken cancellation);
	}
}
