using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public partial class DatabaseQueriesInRowSelectingAnalyzer : LooseEventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1042_DatabaseQueriesInRowSelecting);

		public override bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo) =>
			base.ShouldAnalyze(pxContext, eventHandlerInfo) && 
			eventHandlerInfo.Type == EventType.RowSelecting && !pxContext.IsAcumatica2023R1_OrGreater;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = context.Symbol as IMethodSymbol;
			var methodSyntax = methodSymbol?.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			methodSyntax?.Accept(new DiagnosticWalker(context, pxContext));
		}
	}
}
