using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	/// <summary>
	/// A base non-generic interface for information about graph event handlers related to DAC fields.
	/// </summary>
	public interface IGraphFieldEventHandlerInfo 
	{
		/// <summary>
		/// The DAC name.
		/// </summary>
		public string DacName { get; }

		/// <summary>
		/// The DAC field name.
		/// </summary>
		public string DacFieldName { get; }

		/// <summary>
		/// The type of the signature of the event handler.
		/// </summary>
		/// <value>
		/// The type of the signature.
		/// </value>
		public EventHandlerSignatureType SignatureType { get; }

		/// <summary>
		/// The type of the event.
		/// </summary>
		public EventType EventType { get; }

		/// <summary>
		/// The event target kind
		/// </summary>
		public EventTargetKind TargetKind { get; }

		/// <summary>
		/// The delegate parameter with a combination of previous event handlers.<br/>
		/// Is not null, if the event handler is overriding base events via Acumatica event override mechanism.
		/// </summary>
		public IMethodSymbol? BaseDelegate { get; }
	}
}
