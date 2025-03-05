using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	public static class EventsRecognition
	{
		/// <summary>
		/// Returns event handler type <see cref="EventType"/> for the provided method symbol.
		/// </summary>
		/// <param name="symbol">Method symbol for the event handler</param>
		/// <param name="pxContext">PXContext instance</param>
		/// <returns>
		/// <see cref="EventType"/> (e.g. RowSelecting). If method is not an event handler, returns <see cref="EventType.None"/>.
		/// </returns>
		public static EventType GetEventHandlerType(this IMethodSymbol symbol, PXContext pxContext) =>
			symbol.GetEventHandlerInfo(pxContext).Type;

		/// <summary>
		/// Returns information about an event handler for the provided method symbol.
		/// </summary>
		/// <param name="symbol">Method symbol for the event handler</param>
		/// <param name="pxContext">PXContext instance</param>
		/// <returns>Event Type (e.g. RowSelecting) and Event Signature Type (default or generic).
		/// If method is not an event handler, returns <code>(EventType.None, EventHandlerSignatureType.None)</code>.</returns>
		public static EventHandlerGeneralInfo GetEventHandlerInfo(this IMethodSymbol symbol, PXContext pxContext)
		{
			symbol.ThrowOnNull();
			pxContext.ThrowOnNull();

			if (!symbol.ReturnsVoid || !symbol.TypeParameters.IsDefaultOrEmpty || symbol.Parameters.IsDefaultOrEmpty)
				return EventHandlerGeneralInfo.None;

			ITypeSymbol firstParameterType = symbol.Parameters[0].Type.OriginalDefinition;

			// Loosely check method signature because sometimes business logic 
			// is extracted from event handler calls to a separate method	
			if (firstParameterType.InheritsFromOrEquals(pxContext.PXCache.Type))
			{ 
				return RecognizeEventHandlerWithClassicSyntax(symbol, pxContext); 
			}

			return RecognizeEventHandlerWithGenericSyntax(firstParameterType, pxContext);
		}

		private static EventHandlerGeneralInfo RecognizeEventHandlerWithClassicSyntax(IMethodSymbol candidateSymbol, PXContext pxContext)
		{
			if (candidateSymbol.Name.EndsWith("CacheAttached", StringComparison.Ordinal))
				return new EventHandlerGeneralInfo(EventType.CacheAttached, EventHandlerSignatureType.Classic);

			if (candidateSymbol.Parameters.Length < 2)
				return EventHandlerGeneralInfo.None;

			var secondParameterType = candidateSymbol.Parameters[1].Type.OriginalDefinition;

			if (pxContext.Events.EventTypeMap.TryGetValue(secondParameterType, out EventType eventType))
				return new EventHandlerGeneralInfo(eventType, EventHandlerSignatureType.Classic);
			else
				return EventHandlerGeneralInfo.None;
		}

		private static EventHandlerGeneralInfo RecognizeEventHandlerWithGenericSyntax(ITypeSymbol firstParameterType, PXContext pxContext)
		{
			if (pxContext.Events.EventTypeMap.TryGetValue(firstParameterType, out EventType eventType)) 
			{
				return new EventHandlerGeneralInfo(eventType, EventHandlerSignatureType.Generic);
			}

			return EventHandlerGeneralInfo.None;
		}
	}
}
