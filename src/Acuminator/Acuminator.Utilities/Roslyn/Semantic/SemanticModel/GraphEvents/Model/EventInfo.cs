using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Information about the graph event.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct EventInfo : IEquatable<EventInfo>
{
	public IMethodSymbol Symbol { get; }

	/// <summary>
	/// A flag indicating whether this event handler is a standard C# override.
	/// </summary>
	public bool IsCSharpOverride => Symbol.IsOverride;

	public EventType Type { get; }

	public EventHandlerSignatureType SignatureType { get; }

	public IParameterSymbol? BaseEventHandlerParameter {  get; }

	/// <summary>
	/// A flag indicating whether this event handler is an override based on Acumatica event handlers override mechanism 
	/// that relies on specifying an additional delegate parameter for base event handlers.
	/// </summary>
	[MemberNotNullWhen(returnValue: true, nameof(BaseEventHandlerParameter))]
	public bool IsAcumaticaOverride => BaseEventHandlerParameter != null;

	public EventInfo(IMethodSymbol eventHandlerSymbol, EventType type, EventHandlerSignatureType signatureType, IParameterSymbol? baseEventHandlerParameter)
	{
		Symbol					  = eventHandlerSymbol.CheckIfNull();
		Type 					  = type;
		SignatureType 			  = signatureType;
		BaseEventHandlerParameter = baseEventHandlerParameter;
	}

	///// <summary>
	///// Returns event handler type <see cref="EventType"/> for the provided method symbol.
	///// </summary>
	///// <param name="symbol">Method symbol for the event handler</param>
	///// <param name="pxContext">PXContext instance</param>
	///// <returns>
	///// <see cref="EventType"/> (e.g. RowSelecting). If method is not an event handler, returns <see cref="EventType.None"/>.
	///// </returns>
	//public static EventType GetEventHandlerType(this IMethodSymbol symbol, PXContext pxContext) =>
	//	symbol.GetEventHandlerInfo(pxContext).Type;

	///// <summary>
	///// Returns information about an event handler for the provided method symbol.
	///// </summary>
	///// <param name="symbol">Method symbol for the event handler</param>
	///// <param name="pxContext">PXContext instance</param>
	///// <returns>Event Type (e.g. RowSelecting) and Event Signature Type (default or generic).
	///// If method is not an event handler, returns <code>(EventType.None, EventHandlerSignatureType.None)</code>.</returns>
	//public static EventInfo GetEventHandlerInfo(IMethodSymbol symbol, PXContext pxContext)
	//{
	//	symbol.ThrowOnNull();
	//	pxContext.ThrowOnNull();

	//	if (!symbol.ReturnsVoid || !symbol.TypeParameters.IsDefaultOrEmpty || symbol.Parameters.IsDefaultOrEmpty)
	//		return EventInfo.None(symbol);

	//	// Loosely check method signature because sometimes business logic 
	//	// is extracted from event handler calls to a separate method

	//	// Old non-generic syntax
	//	if (symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCache.Type))
	//	{
	//		if (symbol.Name.EndsWith("CacheAttached", StringComparison.Ordinal))
	//			return new EventInfo(EventType.CacheAttached, EventHandlerSignatureType.Default);

	//		if (symbol.Parameters.Length >= 2 && pxContext.Events.EventTypeMap.TryGetValue(
	//				symbol.Parameters[1].Type.OriginalDefinition, out EventType eventType))
	//		{
	//			return new EventInfo(eventType, EventHandlerSignatureType.Default);
	//		}
	//	}
	//	else if (pxContext.Events.EventTypeMap.TryGetValue(
	//		symbol.Parameters[0].Type.OriginalDefinition, out EventType eventType)) // New generic event handler syntax
	//	{
	//		return new EventInfo(eventType, EventHandlerSignatureType.Generic);
	//	}

	//	return EventInfo.None(symbol);
	//}

	public static EventInfo None(IMethodSymbol eventHandler) => new(eventHandler, EventType.None, EventHandlerSignatureType.None, 
																	baseEventHandlerParameter: null);

	public override bool Equals(object obj) => obj is EventInfo other && Equals(other);

	public bool Equals(EventInfo other) => Symbol.Equals(other.Symbol, SymbolEqualityComparer.Default);

	public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(Symbol);

	public override string ToString() => $"Name: {Symbol.Name}, Type: {Type.ToString()}, Signature Type: {SignatureType.ToString()}, " + 
										 $"Is Override: {IsAcumaticaOverride || IsCSharpOverride}";
}
