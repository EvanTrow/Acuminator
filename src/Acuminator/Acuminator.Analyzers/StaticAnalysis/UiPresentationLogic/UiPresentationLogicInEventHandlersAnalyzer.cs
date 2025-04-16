
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic
{
	public class UiPresentationLogicInEventHandlersAnalyzer : LooseEventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1070_UiPresentationLogicInEventHandlers);

		public override bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo) =>
			base.ShouldAnalyze(pxContext, eventHandlerInfo) &&
			eventHandlerInfo.Type is not (EventType.RowSelected or EventType.CacheAttached);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, Descriptors.PX1070_UiPresentationLogicInEventHandlers);

			methodSyntax?.Accept(walker);
		}
	}
}
