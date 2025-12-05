
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInActionHandlersAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var walker = new WalkerForGraphAnalyzer(context, pxContext, Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers);
			var delegateNodes = pxGraph.DeclaredActionDelegates
									   .Where(actionDelegate => actionDelegate.Node != null)
									   .Select(actionDelegate => actionDelegate.Node);

			foreach (var node in delegateNodes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				walker.Visit(node);
			}
		}
	}
}
