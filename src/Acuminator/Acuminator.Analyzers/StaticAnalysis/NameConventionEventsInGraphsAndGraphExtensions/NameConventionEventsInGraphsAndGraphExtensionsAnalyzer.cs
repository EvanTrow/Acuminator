
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	public class NameConventionEventsInGraphsAndGraphExtensionsAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graph) => 
			base.ShouldAnalyze(pxContext, graph) && graph.DeclaredEventHandlers.AllEventHandlersCount > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel graphOrExtensionWithEvents)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var allDeclaredNamingConventionEventHandlers = from eventHandler in graphOrExtensionWithEvents.DeclaredEventHandlers.GetAllEventHandlers()
														   where eventHandler.SignatureType == EventHandlerSignatureType.Classic
														   select eventHandler;

			foreach (GraphEventHandlerInfoBase handlerInfo in allDeclaredNamingConventionEventHandlers)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (IsSuitableForConversionToGenericSignature(handlerInfo, pxContext))
				{
					ReportDiagnosticForEventHandler(symbolContext, pxContext, handlerInfo);
				}
			}
		}

		private static void ReportDiagnosticForEventHandler(SymbolAnalysisContext symbolContext, PXContext pxContext, GraphEventHandlerInfoBase handlerInfo)
		{
			// Node is not null here because aggregated graph analyzers work only on graphs and graph extensions declared in the source code,
			// and only events declared in the graph or graph extension are analyzed
			var graphEventHandlerLocation = handlerInfo.Node!.Identifier.GetLocation();
			var properties = new Dictionary<string, string?>
			{
				{ NameConventionEventsInGraphsAndGraphExtensionsDiagnosticProperties.EventType, handlerInfo.EventType.ToString() },
				{ DiagnosticProperty.DacName, handlerInfo.DacName }
			};

			if (handlerInfo is IGraphFieldEventHandlerInfo graphFieldEventHandler)
				properties.Add(DiagnosticProperty.DacFieldName, graphFieldEventHandler.DacFieldName);

			symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions, graphEventHandlerLocation,
									  properties: properties.ToImmutableDictionary()),
					pxContext.CodeAnalysisSettings);
		}

		private bool IsSuitableForConversionToGenericSignature(GraphEventHandlerInfoBase eventHandlerInfo, PXContext pxContext)
		{
			// event handlers with more than 2 parameters should be overrides which shouldn't be converted to generic events
			// as well as C# overrides of base events
			if (eventHandlerInfo.Symbol.Parameters.Length > 2 || eventHandlerInfo.IsCSharpOverride)
				return false;

			// PXOverridden events can't be converted either.
			// We don't need to check for PXOverride attribute on overridden methods, because we already filtered out C# overrides in the previous check
			if (eventHandlerInfo.IsPXOverride)
				return false;

			// check that there is a corresponding generic event args symbol
			var eventHandlerInfoForGenericSignature = new EventHandlerLooseInfo(eventHandlerInfo.EventType, EventHandlerSignatureType.Generic);
			return pxContext.Events.EventHandlerSignatureTypeMap.ContainsKey(eventHandlerInfoForGenericSignature);
		}
	}
}
