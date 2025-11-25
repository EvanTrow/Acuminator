
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersAnalyzer : LooseEventHandlerAggregatedAnalyzerBase
	{
		private static readonly EventType[] AnalyzedEventTypes =
		{
			EventType.FieldDefaulting,
			EventType.FieldVerifying,
			EventType.RowSelected,
			EventType.RowSelecting,
		};

		private static readonly EventType[] AnalyzedEventTypesForIsv = 
			AnalyzedEventTypes
				.AppendItem(EventType.RowInserting)
				.AppendItem(EventType.RowUpdating)
				.AppendItem(EventType.RowDeleting)
				.ToArray();

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1044_ChangesInPXCacheInEventHandlers);

		public override bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			if (!base.ShouldAnalyze(pxContext, eventHandlerInfo))
				return false;

			var eventSet = pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
				? AnalyzedEventTypesForIsv
				: AnalyzedEventTypes;

			return eventSet.Contains(eventHandlerInfo.Type);
		}

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, Descriptors.PX1044_ChangesInPXCacheInEventHandlers, eventHandlerInfo.Type);

			methodSyntax?.Accept(walker);
		}
	}
}
