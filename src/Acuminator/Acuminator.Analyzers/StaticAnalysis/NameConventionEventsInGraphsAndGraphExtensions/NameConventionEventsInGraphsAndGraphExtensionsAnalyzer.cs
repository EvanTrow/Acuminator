
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel graphOrExtensionWithEvents)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var allDeclaredNamingConventionEvents = from @event in graphOrExtensionWithEvents.GetAllEvents()
													where @event.SignatureType == EventHandlerSignatureType.Classic &&
														  @event.Symbol.IsDeclaredInType(graphOrExtensionWithEvents.Symbol)
													select @event;

			INamedTypeSymbol pxOverrideAttribute = pxContext.AttributeTypes.PXOverrideAttribute;

			foreach (GraphEventHandlerInfoBase eventInfo in allDeclaredNamingConventionEvents)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (IsSuitableForConversionToGenericSignature(eventInfo, pxContext, pxOverrideAttribute))
				{
					ReportDiagnosticForEvent(symbolContext, pxContext, eventInfo);
				}
			}
		}

		private static void ReportDiagnosticForEvent(SymbolAnalysisContext symbolContext, PXContext pxContext, GraphEventHandlerInfoBase eventInfo)
		{
			// Node is not null here because aggregated graph analyzers work only on graphs and graph extensions declared in the source code,
			// and only events declared in the graph or graph extension are analyzed
			var graphEventLocation = eventInfo.Node!.Identifier.GetLocation();
			var properties = new Dictionary<string, string?>
			{
				{ NameConventionEventsInGraphsAndGraphExtensionsDiagnosticProperties.EventType, eventInfo.EventType.ToString() },
				{ DiagnosticProperty.DacName, eventInfo.DacName }
			};

			if (eventInfo is IGraphFieldEventHandlerInfo graphFieldEventHandler)
				properties.Add(DiagnosticProperty.DacFieldName, graphFieldEventHandler.DacFieldName);

			symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions, graphEventLocation,
									  properties: properties.ToImmutableDictionary()),
					pxContext.CodeAnalysisSettings);
		}

		private bool IsSuitableForConversionToGenericSignature(GraphEventHandlerInfoBase eventHandlerInfo, PXContext pxContext, INamedTypeSymbol pxOverrideAttribute)
		{
			// event handlers with more than 2 parameters should be overrides which shouldn't be converted to generic events
			// as well as C# overrides of base events
			if (eventHandlerInfo.Symbol.Parameters.Length > 2 || eventHandlerInfo.IsCSharpOverride)
				return false;

			var eventAttributes	= eventHandlerInfo.Symbol.GetAttributes();

			// PXOverridden events can't be converted either.
			// We don't need to check for PXOverride attribute on overridden methods, because we already filtered out C# overrides in the previous check
			if (!eventAttributes.IsDefaultOrEmpty && eventAttributes.Any(a => pxOverrideAttribute.Equals(a.AttributeClass, SymbolEqualityComparer.Default)))
				return false;

			// check that there is a corresponding generic event args symbol
			var eventHandlerInfoForGenericSignature = new EventHandlerLooseInfo(eventHandlerInfo.EventType, EventHandlerSignatureType.Generic);
			return pxContext.Events.EventHandlerSignatureTypeMap.ContainsKey(eventHandlerInfoForGenericSignature);
		}
	}
}
