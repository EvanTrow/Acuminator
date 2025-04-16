using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Information about Acumatica loosely recognized event handler.
/// </summary>
/// <remarks>
/// Event handlers described by this structure are loosely recognized event handlers.<br/>
/// This means that <see cref="EventHandlerLooseInfo"/> describes not only event handlers from graphs and graph extensions but also event handlers from attributes and general helpers.<br/>
/// Such event handlers are reconized by Acumiantor by their signature. Loosely recognition means that:
/// <list type="bullet">
/// <item>Event handler method can be declared outside of graphs and graph extensions.</item>
/// <item>Event handler's parameters list may have additional parameters besided the standard parameters for classic and generic syntaxes.<br/>
/// However, the standard parameters must be declared first and in the same order as they are in the standard syntaxes for graph event handlers.</item>
/// </list>
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public readonly struct EventHandlerLooseInfo : IEquatable<EventHandlerLooseInfo>
{
	public static EventHandlerLooseInfo None { get; } = new(EventType.None, EventHandlerSignatureType.None);

	public EventType Type { get; }

	public EventHandlerSignatureType SignatureType { get; }

	public EventTargetKind TargetKind { get; }

	public EventCollectionMode CollectionMode => Type.GetEventCollectionMode();

	public EventHandlerLooseInfo(EventType type, EventHandlerSignatureType signatureType)
	{
		Type 		  = type;
		SignatureType = signatureType;
		TargetKind 	  = Type.GetEventTargetKindByEventType();
	}

	public override bool Equals(object obj) => obj is EventHandlerLooseInfo other && Equals(other);

	public bool Equals(EventHandlerLooseInfo other) => Type == other.Type && SignatureType == other.SignatureType;

	public override int GetHashCode()
	{
		int hash = 17;

		unchecked
		{
			hash = 23 * hash + Type.GetHashCode();
			hash = 23 * hash + SignatureType.GetHashCode();
		}

		return hash;
	}

	public override string ToString() => $"Type: {Type.ToString()}, Signature Type: {SignatureType.ToString()}";
}
