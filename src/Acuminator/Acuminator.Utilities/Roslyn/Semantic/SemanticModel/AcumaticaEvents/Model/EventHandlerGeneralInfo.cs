using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// General information defining the category of Acumatica event handler.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct EventHandlerGeneralInfo : IEquatable<EventHandlerGeneralInfo>
{
	public static EventHandlerGeneralInfo None { get; } = new(EventType.None, EventHandlerSignatureType.None);

	public EventType Type { get; }

	public EventHandlerSignatureType SignatureType { get; }

	public EventTargetKind TargetKind { get; }

	public EventHandlerGeneralInfo(EventType type, EventHandlerSignatureType signatureType)
	{
		Type 		  = type;
		SignatureType = signatureType;
		TargetKind 	  = Type.GetEventTargetKindByEventType();
	}

	public override bool Equals(object obj) => obj is EventHandlerGeneralInfo other && Equals(other);

	public bool Equals(EventHandlerGeneralInfo other) => Type == other.Type && SignatureType == other.SignatureType;

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
