using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
	public class LongOperationInGraphAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization,
				Descriptors.PX1080_DataViewDelegateLongOperationStart
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDataViewsForLongRunOperations(context, pxContext, pxGraphOrGraphExt);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckGraphInitializationForLongRunOperations(context, pxContext, pxGraphOrGraphExt);
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
	}
}
