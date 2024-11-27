using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.Roslyn.Semantic.EventsRecognition
{
	public static class EventsRecognitionUtils
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

			if (symbol.ReturnsVoid && symbol.TypeParameters.IsEmpty && !symbol.Parameters.IsEmpty)
			{
				// Loosely check method signature because sometimes business logic 
				// is extracted from event handler calls to a separate method

				// Old non-generic syntax
				if (symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCache.Type))
				{
					if (symbol.Name.EndsWith("CacheAttached", StringComparison.Ordinal))
						return new EventInfo(EventType.CacheAttached, EventHandlerSignatureType.Default);

					if (symbol.Parameters.Length >= 2 && pxContext.Events.EventTypeMap.TryGetValue(
						    symbol.Parameters[1].Type.OriginalDefinition, out EventType eventType))
					{
						return new EventInfo(eventType, EventHandlerSignatureType.Default);
					}
				}
				else if (pxContext.Events.EventTypeMap.TryGetValue(
					symbol.Parameters[0].Type.OriginalDefinition, out EventType eventType)) // New generic event handler syntax
				{
					return new EventInfo(eventType, EventHandlerSignatureType.Generic);
				}
			}

			return EventInfo.None();
		}
	}
}
