using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Analyzers.StaticAnalysis.RaiseExceptionHandling;
using Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
	/// <summary>
	/// An event handler aggregator analyzer. This analyzer validates loosely recognized event handlers.<br/>
	/// Loosely recognized event handlers are not only event handlers from graphs and graph extensions but also event handlers from attributes and other helpers.<br/>
	/// You can see more info about loose event handlers in <see cref="EventHandlerGeneralInfo"/>.<br/>
	/// </summary>
	/// <remarks>
	/// For analysis of strictly recognized event handlers from graphs and graph extensions and aggregated graph analyzer should be written.<br/>
	/// See <see cref="PXGraph.IPXGraphAnalyzer"/> and <see cref="PXGraph.PXGraphAnalyzer"/> for details.
	/// </remarks>
	/// <seealso cref="EventHandlerGeneralInfo"/>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EventHandlerAggregatorAnalyzer : SymbolAnalyzersAggregator<IEventHandlerAnalyzer>
	{
        protected override SymbolKind SymbolKind => SymbolKind.Method;

		public EventHandlerAggregatorAnalyzer() : this(null,
			// can be replaced with DI from ServiceLocator if DI-container is used
			new DatabaseQueriesInRowSelectingAnalyzer(),
			new SavingChangesInEventHandlersAnalyzer(),
			new ChangesInPXCacheInEventHandlersAnalyzer(),
			new PXGraphCreateInstanceInEventHandlersAnalyzer(),
			new LongOperationInEventHandlersAnalyzer(),
			new RowChangesInEventHandlersAnalyzer(),
			new DatabaseQueriesInRowSelectedAnalyzer(),
			new UiPresentationLogicInEventHandlersAnalyzer(),
			new PXActionExecutionInEventHandlersAnalyzer(),
			new ThrowingExceptionsInEventHandlersAnalyzer(),
			new RaiseExceptionHandlingInEventHandlersAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public EventHandlerAggregatorAnalyzer(CodeAnalysisSettings? settings, params IEventHandlerAnalyzer[] innerAnalyzers)
            : base(settings, innerAnalyzers)
		{
		}

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not IMethodSymbol methodSymbol)
				return;
		
			EventType eventType = methodSymbol.GetEventHandlerType(pxContext);

			if (eventType == EventType.None)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();
			var effectiveEventAnalyzers = _innerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, eventType))
														 .ToList(capacity: _innerAnalyzers.Length);

			RunAggregatedAnalyzersInParallel(effectiveEventAnalyzers, context, analyzerIndex =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var aggregatedAnalyzer = effectiveEventAnalyzers[analyzerIndex];
				aggregatedAnalyzer.Analyze(context, pxContext, eventType);
			});
		}

		//private void AnalyzeLambda(OperationAnalysisContext context, PXContext pxContext)
		//{
		//	if (context.Operation is ILambdaExpression lambdaExpression)
		//	{
		//		var symbolAnalysisContext =
		//			new SymbolAnalysisContext(lambdaExpression.Signature, context.Compilation, context.Options,
		//									  context.ReportDiagnostic, d => true, // this check is covered inside context.ReportDiagnostic
		//									  context.CancellationToken);

		//		AnalyzeSymbol(symbolAnalysisContext, pxContext);
		//	}
		//}
	}
}
