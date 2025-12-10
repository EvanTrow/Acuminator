using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes;
using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseDataViewDelegate;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraph;
using Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField;
using Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.ObsoleteElementsUsage;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewAndActionHandlerName;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Graph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXGraphAnalyzer : SymbolAnalyzersAggregator<IPXGraphAnalyzer>
	{
		protected override SymbolKind SymbolKind => SymbolKind.NamedType;

		public PXGraphAnalyzer() : this(null,
			new GraphAndGraphExtensionDeclarationAnalyzer(),
			new PXGraphCreationInGraphInWrongPlacesGraphAnalyzer(),
			new ConstructorInGraphExtensionAnalyzer(),
			new SavingChangesInGraphSemanticModelAnalyzer(),
			new ChangesInPXCacheDuringPXGraphInitializationAnalyzer(),
			new LongOperationInGraphAnalyzer(),
			new PXActionExecutionInGraphSemanticModelAnalyzer(),
			new DatabaseQueriesInPXGraphInitializationAnalyzer(),
			new ThrowingExceptionsInLongRunningOperationAnalyzer(),
			new ThrowingExceptionsInActionHandlersAnalyzer(),
			new NoIsActiveMethodForExtensionAnalyzer(),
			new NameConventionEventsInGraphsAndGraphExtensionsAnalyzer(),
			new ThrowingExceptionsInEventHandlersAnalyzer(),
			new CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer(),
			new CallingBaseActionHandlerFromOverrideHandlerAnalyzer(),
			new UiPresentationLogicInActionHandlersAnalyzer(),
			new ViewDeclarationOrderAnalyzer(),
			new NoPrimaryViewForPrimaryDacAnalyzer(),
			new ActionHandlerAttributesAnalyzer(),
			new NonPublicGraphAndDacAndExtensionsAnalyzer(),
			new InvalidPXActionSignatureAnalyzer(),
			new StaticFieldOrPropertyInGraphAnalyzer(),
			new TypoInViewAndActionHandlerNameAnalyzer(),
			new PXOverrideAnalyzer(),
			new MainDacOfProcessingViewMustContainNoteIdFieldAnalyzer(),
			new ObsoleteElementsUsageAnalyzer(),
			new ForbidPrivateEventHandlersAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public PXGraphAnalyzer(CodeAnalysisSettings? settings, params IPXGraphAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
		{
		}

		protected override IReadOnlyCollection<DiagnosticDescriptor> GetAggregatorOwnDiagnostics(CodeAnalysisSettings? settings) =>
			new[]
			{
				Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension,
				Descriptors.PX1117_GraphExtensionExtendsTwoGraphs,
				Descriptors.PX1117_GraphExtensionExtends_3_To_5_Graphs,
				Descriptors.PX1117_GraphExtensionExtendsMoreThanFiveGraphs,
			}
			.Distinct()
			.ToList(capacity: 4);

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not INamedTypeSymbol type)
				return;

			var graphOrGraphExtInfo = InferSymbolInfo(context, pxContext, type);

			if (graphOrGraphExtInfo == null)
				return;

			var graphOrGraphExtModel = PXGraphEventSemanticModel.InferModel(pxContext, graphOrGraphExtInfo, GraphSemanticModelCreationOptions.CollectAll,
																			cancellation: context.CancellationToken);
			if (graphOrGraphExtModel == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();

			var effectiveAnalyzers = InnerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, graphOrGraphExtModel))
												   .ToList(capacity: InnerAnalyzers.Length);

			RunAggregatedAnalyzersInParallel(effectiveAnalyzers, context, aggregatedAnalyzerAction: analyzerIndex =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var analyzer = effectiveAnalyzers[analyzerIndex];
				analyzer.Analyze(context, pxContext, graphOrGraphExtModel);
			});
		}

		private GraphOrGraphExtInfoBase? InferSymbolInfo(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type)
		{
			InferredSymbolInfo? inferredInfo = GraphAndGraphExtInfoBuilder.Instance.InferTypeInfo(type, pxContext, customDeclarationOrder: null, 
																								  context.CancellationToken);
			if (inferredInfo == null)
				return null;

			InferResultKind resultKind = inferredInfo.GetResultKind();

			switch (resultKind)
			{
				case InferResultKind.MultipleRootTypes:
					ReportMultipleRoots(context, pxContext, type, inferredInfo.CollectedRootTypes);
					return null;
				case InferResultKind.CircularReferences:
					ReportCircularExtensions(context, pxContext, type, inferredInfo.CircularReferenceExtension!);
					return null;

				case InferResultKind.Success:
					return inferredInfo.InferredInfo as GraphOrGraphExtInfoBase;

				// For unknown errors and bad base graph extensions in graphs we do not report anything
				case InferResultKind.BadBaseExtensions:
				case InferResultKind.UnrecognizedError:
				default:
					return null;
			}
		}

		private void ReportMultipleRoots(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type, 
										 IReadOnlyCollection<ITypeSymbol> multipleRootTypes)
		{
			var diagnostic = MultipleGraphsDiagnosticFactory.Instance.CreateDiagnosticForMultipleRootSymbols(type,
																			multipleRootTypes.ToList(capacity: multipleRootTypes.Count));
			if (diagnostic != null)
			{
				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private void ReportCircularExtensions(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type, ITypeSymbol circularExtension)
		{
			if (!type.IsInSourceCode())
				return;

			var location = type.Locations.FirstOrDefault();
			var diagnostic = Diagnostic.Create(Descriptors.PX1116_CircularReferenceInTypeHierarchy_GraphExtension, location,
											   [type.Name, circularExtension.ToString()]);
			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
