using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
    public interface ILooseEventHandlerAggregatedAnalyzer : ISymbolAnalyzer
	{
		void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo);

		/// <summary>
		/// Determine if the analyzer should run for event type.
		/// </summary>
		/// <param name="pxContext">Context.</param>
		/// <param name="eventHandlerInfo">Information about the loose event handler.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo);
	}
}
