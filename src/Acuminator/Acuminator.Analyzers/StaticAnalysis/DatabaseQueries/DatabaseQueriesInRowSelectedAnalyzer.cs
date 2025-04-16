
using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
    public class DatabaseQueriesInRowSelectedAnalyzer : LooseEventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1049_DatabaseQueriesInRowSelected);

		public override bool ShouldAnalyze(PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo) =>
			base.ShouldAnalyze(pxContext, eventHandlerInfo) && 
			pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled && eventHandlerInfo.Type == EventType.RowSelected;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			methodSyntax?.Accept(new Walker(context, pxContext, Descriptors.PX1049_DatabaseQueriesInRowSelected));
		}
	}
}
