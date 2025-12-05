using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
	public abstract class SymbolAnalyzersAggregator<T> : PXDiagnosticAnalyzer
	where T : ISymbolAnalyzer
	{
		protected ImmutableArray<T> InnerAnalyzers { get; }

		protected abstract SymbolKind SymbolKind { get; }

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		protected SymbolAnalyzersAggregator(CodeAnalysisSettings? settings, params T[] innerAnalyzers) : base(settings)
		{
			InnerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
			SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
		}

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeSymbolHandleAggregateException(c, pxContext), SymbolKind);
			// TODO: Enable this operation action after migration to Roslyn v2
			//compilationStartContext.RegisterOperationAction(c => AnalyzeLambda(c, pxContext, codeAnalysisSettings), OperationKind.LambdaExpression);
		}

		private void AnalyzeSymbolHandleAggregateException(SymbolAnalysisContext context, PXContext pxContext)
		{
			try
			{
				AnalyzeSymbol(context, pxContext);
			}
			catch (AggregateException e)
			{
				var operationCanceledException = e.Flatten().InnerExceptions
					.OfType<OperationCanceledException>()
					.FirstOrDefault();

				if (operationCanceledException != null)
				{
					throw operationCanceledException;
				}

				throw;
			}
		}

		protected abstract void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext);

		protected virtual void RunAggregatedAnalyzersInParallel(List<T> effectiveAnalyzers, SymbolAnalysisContext context,
																Action<int> aggregatedAnalyserAction, ParallelOptions? parallelOptions = null)
		{
			switch (effectiveAnalyzers.Count)
			{
				case 0:
					return;
				case 1:
					aggregatedAnalyserAction(0);
					return;
				default:
					if (Debugger.IsAttached)
					{
						for (int analyzerIndex = 0; analyzerIndex < effectiveAnalyzers.Count; analyzerIndex++)
						{
							aggregatedAnalyserAction(analyzerIndex);
						}
					}
					else
					{
						RunInParallel(effectiveAnalyzers, context, aggregatedAnalyserAction, parallelOptions);
					}

					return;
			}
		}

		private void RunInParallel(List<T> effectiveAnalyzers, SymbolAnalysisContext context, Action<int> aggregatedAnalyserAction,
								   ParallelOptions? parallelOptions)
		{
			parallelOptions = parallelOptions ?? new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			try
			{
				Parallel.For(0, effectiveAnalyzers.Count, parallelOptions, aggregatedAnalyserAction);
			}
			catch (AggregateException aggregateException)
			{
				var unwrappedException = UnwrapAggregatedException(aggregateException);

				if (unwrappedException != null)
					ExceptionDispatchInfo.Capture(unwrappedException).Throw();

				throw;
			}
		}

		private Exception? UnwrapAggregatedException(AggregateException aggregateException)
		{
			switch (aggregateException.InnerExceptions.Count)
			{
				case 0:
					return null;

				case 1
				when aggregateException.InnerExceptions[0] is not AggregateException:	// Hot path
					return aggregateException.InnerExceptions[0];

				default:
					var flattenedException = aggregateException.Flatten();
					return flattenedException.InnerExceptions.Count switch
					{
						0 => null,
						1 => flattenedException.InnerExceptions[0],
						_ => flattenedException
					};
			}
		}
	}
}
