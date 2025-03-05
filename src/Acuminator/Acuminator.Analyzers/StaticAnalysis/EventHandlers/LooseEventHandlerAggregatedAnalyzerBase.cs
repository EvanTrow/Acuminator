using System.Collections.Immutable;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
	/// <summary>
	/// Base class for aggregated loosely recognized event handler analyzers.
	/// </summary>
	public abstract class LooseEventHandlerAggregatedAnalyzerBase : ILooseEventHandlerAggregatedAnalyzer
	{
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo);

		public virtual bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo) => 
			eventHandlerInfo.Type != EventType.None;
	}
}
