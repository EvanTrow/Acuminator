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
		public static EventInfo GetEventHandlerInfo(this IMethodSymbol symbol, PXContext pxContext)
		{
			symbol.ThrowOnNull();
			pxContext.ThrowOnNull();

			if (!symbol.ReturnsVoid || !symbol.TypeParameters.IsDefaultOrEmpty || symbol.Parameters.IsDefaultOrEmpty)
				return EventInfo.None(symbol);

			// Loosely check method signature because sometimes business logic 
			// is extracted from event handler calls to a separate method

			// Old non-generic syntax
			if (symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCache.Type))
			{
				if (symbol.Name.EndsWith("CacheAttached", StringComparison.Ordinal))
					return new EventInfo(symbol, EventType.CacheAttached, EventHandlerSignatureType.Default, null);

				if (symbol.Parameters.Length >= 2 && pxContext.Events.EventTypeMap.TryGetValue(
						symbol.Parameters[1].Type.OriginalDefinition, out EventType eventType))
				{
					return new EventInfo(symbol, eventType, EventHandlerSignatureType.Default, null);
				}
			}
			else if (pxContext.Events.EventTypeMap.TryGetValue(
				symbol.Parameters[0].Type.OriginalDefinition, out EventType eventType)) // New generic event handler syntax
			{
				return new EventInfo(symbol, eventType, EventHandlerSignatureType.Generic, null);
			}

			return EventInfo.None(symbol);
		}
	}
}
