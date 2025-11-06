using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
	public class LongOperationInGraphAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1013_PXActionHandlerInvalidReturnType,
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization,
				Descriptors.PX1080_DataViewDelegateLongOperationStart
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDataViewsForLongRunOperations(context, pxContext, pxGraphOrGraphExt);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckGraphInitializationForLongRunOperations(context, pxContext, pxGraphOrGraphExt);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckActionDelegateStartingLongRunOperationsHasCorrectSignatures(context, pxContext, pxGraphOrGraphExt);
		}

		protected virtual void CheckDataViewsForLongRunOperations(SymbolAnalysisContext context, PXContext pxContext, 
																  PXGraphEventSemanticModel pxGraphOrGraphExt)
		{
			if (pxGraphOrGraphExt.ViewDelegatesByNames.Count == 0)
				return;

			var walker = new StartLongOperationWalker(context, pxContext, Descriptors.PX1080_DataViewDelegateLongOperationStart);

			foreach (DataViewDelegateInfo viewDelegate in pxGraphOrGraphExt.DeclaredViewDelegates)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (viewDelegate.Node != null)
					walker.Visit(viewDelegate.Node);
			}
		}

		protected virtual void CheckGraphInitializationForLongRunOperations(SymbolAnalysisContext context, PXContext pxContext,
																			PXGraphEventSemanticModel pxGraphOrGraphExt)
		{
			if (pxGraphOrGraphExt.DeclaredInitializers.IsDefaultOrEmpty)
				return;

			var walker = new StartLongOperationWalker(context, pxContext, Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization);

			foreach (GraphInitializerInfo initializer in pxGraphOrGraphExt.DeclaredInitializers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (initializer.Node != null)
					walker.Visit(initializer.Node);
			}

			if (pxGraphOrGraphExt.GraphType != GraphType.PXGraphExtension)
				return;

			if (pxGraphOrGraphExt.IsActiveMethodInfo?.Node != null)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				walker.Visit(pxGraphOrGraphExt.IsActiveMethodInfo.Node);
			}

			if (pxGraphOrGraphExt.IsActiveForGraphMethodInfo?.Node != null)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				walker.Visit(pxGraphOrGraphExt.IsActiveForGraphMethodInfo.Node);
			}
		}

		protected virtual void CheckActionDelegateStartingLongRunOperationsHasCorrectSignatures(SymbolAnalysisContext context, PXContext pxContext,
																								PXGraphEventSemanticModel pxGraphOrGraphExt)
		{
			if (pxGraphOrGraphExt.ActionHandlersByNames.Count == 0)
				return;

			foreach (var actionHandler in pxGraphOrGraphExt.DeclaredActionHandlers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (actionHandler.Node != null)
				{
					CheckActionHandlerReturnType(context, pxContext, actionHandler);
				}
			}
		}

		private void CheckActionHandlerReturnType(SymbolAnalysisContext context, PXContext pxContext, ActionHandlerInfo actionHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (pxContext.SystemTypes.IEnumerable.Equals(actionHandlerInfo.Symbol.ReturnType, SymbolEqualityComparer.Default) ||
				!StartsLongOperation(pxContext, actionHandlerInfo.Node!, context.CancellationToken))
			{
				return;
			}

			var location = actionHandlerInfo.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ??
						   actionHandlerInfo.Symbol.Locations.FirstOrDefault();
			var diagnostic = Diagnostic.Create(Descriptors.PX1013_PXActionHandlerInvalidReturnType,
											  location);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private bool StartsLongOperation(PXContext pxContext, SyntaxNode node, CancellationToken cancellation)
		{
			var walker = new StartLongOperationDelegateWalker(pxContext, cancellation);

			walker.Visit(node);

			return walker.Delegates.Length > 0;
		}
	}
}
