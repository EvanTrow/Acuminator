using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInLongRunningOperationAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var pxSetupNotEnteredExceptionReportingWalker = 
				new WalkerForGraphAnalyzer(context, pxContext, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation);

			CheckProcessingDelegates(graphOrGraphExt, pxSetupNotEnteredExceptionReportingWalker, context.CancellationToken);
			CheckLongOperationStartDelegates(graphOrGraphExt, pxSetupNotEnteredExceptionReportingWalker, pxContext, context.CancellationToken);
		}

		private void CheckProcessingDelegates(PXGraphEventSemanticModel graphOrGraphExt, WalkerForGraphAnalyzer pxSetupNotEnteredExceptionReportingWalker,
											  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (!graphOrGraphExt.IsProcessing)
				return;

			var processingViews = graphOrGraphExt.Views.Where(v => v.IsProcessing);

			foreach (var viewDel in processingViews)
			{
				if (!viewDel.FinallyProcessDelegates.IsDefaultOrEmpty)
				{
					var finallyDelegates = viewDel.FinallyProcessDelegates.Where(d => d.Node != null);

					foreach (var finDel in finallyDelegates)
					{
						cancellation.ThrowIfCancellationRequested();
						pxSetupNotEnteredExceptionReportingWalker.VisitProcessingDelegate(finDel);
					}
				}

				if (!viewDel.ParametersDelegates.IsDefaultOrEmpty)
				{
					var parametersDelegates = viewDel.ParametersDelegates.Where(d => d.Node != null);

					foreach (var parDel in parametersDelegates)
					{
						cancellation.ThrowIfCancellationRequested();
						pxSetupNotEnteredExceptionReportingWalker.VisitProcessingDelegate(parDel);
					}
				}

				if (!viewDel.ProcessDelegates.IsDefaultOrEmpty)
				{
					var processDelegates = viewDel.ProcessDelegates.Where(d => d.Node != null);

					foreach (var processDel in processDelegates)
					{
						cancellation.ThrowIfCancellationRequested();
						pxSetupNotEnteredExceptionReportingWalker.VisitProcessingDelegate(processDel);
					}
				}
			}
		}

		private void CheckLongOperationStartDelegates(PXGraphEventSemanticModel graphOrGraphExt, 
													  WalkerForGraphAnalyzer pxSetupNotEnteredExceptionReportingWalker,
													  PXContext pxContext, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var longRunDelegateNodeList = GetLongOperationStartDelegates(graphOrGraphExt, pxContext, cancellation);

			foreach (var (longRunDelegateSymbol, longRunDelegateNode) in longRunDelegateNodeList)
			{
				cancellation.ThrowIfCancellationRequested();
				pxSetupNotEnteredExceptionReportingWalker.VisitLongRunDelegate(longRunDelegateNode, longRunDelegateSymbol);
			}
		}

		private List<(ISymbol? DelegateSymbol, SyntaxNode? DelegateNode)> GetLongOperationStartDelegates(PXGraphEventSemanticModel graphOrGraphExt, 
																								PXContext pxContext, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var longRunDelegateNodeList = new List<(ISymbol? DelegateSymbol, SyntaxNode? DelegateNode)>();
			var declaringNodes = graphOrGraphExt.Node != null
				? [graphOrGraphExt.Node]
				: graphOrGraphExt.Symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(cancellation));

			foreach (var node in declaringNodes)
			{
				cancellation.ThrowIfCancellationRequested();

				var longRunDelegatesCollectionWalker = new StartLongOperationDelegateWalker(pxContext, cancellation);

				longRunDelegatesCollectionWalker.Visit(node);
				longRunDelegateNodeList.AddRange(longRunDelegatesCollectionWalker.DelegateInfos);
			}

			return longRunDelegateNodeList;
		}
	}
}
