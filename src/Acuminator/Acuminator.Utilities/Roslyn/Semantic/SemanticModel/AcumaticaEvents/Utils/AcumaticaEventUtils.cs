#nullable enable

using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	public static class AcumaticaEventUtils
	{
		/// <summary>
		/// Get event target kind by event type.
		/// </summary>
		/// <param name="eventType">The event type to check.</param>
		/// <returns>
		/// The event target kind from the given event type.
		/// </returns>
		public static EventTargetKind GetEventTargetKindByEventType(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
				case EventType.CacheAttached:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return EventTargetKind.Field;

				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return EventTargetKind.Row;

				case EventType.None:
				default:
					return EventTargetKind.None;
			}
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC field event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacFieldEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.FieldSelecting:
				case EventType.FieldDefaulting:
				case EventType.FieldVerifying:
				case EventType.FieldUpdating:
				case EventType.FieldUpdated:
				case EventType.CacheAttached:
				case EventType.CommandPreparing:
				case EventType.ExceptionHandling:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Check if <paramref name="eventType"/> is DAC row event.
		/// </summary>
		/// <param name="eventType">The eventType to check.</param>
		/// <returns/>
		public static bool IsDacRowEvent(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.RowSelecting:
				case EventType.RowSelected:
				case EventType.RowInserting:
				case EventType.RowInserted:
				case EventType.RowUpdating:
				case EventType.RowUpdated:
				case EventType.RowDeleting:
				case EventType.RowDeleted:
				case EventType.RowPersisting:
				case EventType.RowPersisted:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Check if <see cref="eventCandidate"/> has a valid graph event handler signature.<br/>
		/// <see cref="CodeResolvingUtils.GetEventHandlerInfo"/> helper allows not only graph events but also helper methods with appropriate signature.<br/>
		/// However, for graph events semantic model we are interested only in graph events, so we need to rule out helper methods by checking their signature.
		/// </summary>
		/// <param name="eventHandlerCandidate">The method to check.</param>
		/// <param name="signatureType">Type of the signature.</param>
		/// <param name="eventType">The event type to check - row deleted, field selecting, etc.</param>
		/// <param name="eventTargetKind">The event target kind.</param>
		/// <remarks>
		/// This method should be run only on already recognized event handlers since it does not check types of handler parameters.
		/// </remarks>
		/// <returns>
		/// True if valid graph event signature, false if not.
		/// </returns>
		internal static bool IsValidGraphEventHandlerSignature(this IMethodSymbol eventHandlerCandidate, EventHandlerSignatureType signatureType, 
															 EventType eventType, EventTargetKind eventTargetKind)
		{
			int parametersCount = eventHandlerCandidate.Parameters.Length;

			if (eventHandlerCandidate.CheckIfNull().IsStatic || !eventHandlerCandidate.ReturnsVoid || eventHandlerCandidate.IsGenericMethod ||
				parametersCount < 1 || parametersCount > 3 || eventTargetKind == EventTargetKind.None)
			{ 
				return false; 
			}

			return signatureType switch
			{
				EventHandlerSignatureType.Classic => IsValidClassicGraphEventHandlerSignature(eventHandlerCandidate, eventType, eventTargetKind, 
																							  parametersCount),
				EventHandlerSignatureType.Generic => IsValidGenericGraphEventHandlerSignature(eventHandlerCandidate, parametersCount),
				EventHandlerSignatureType.None 	  => false,
				_ 								  => false
			};
		}

		private static bool IsValidClassicGraphEventHandlerSignature(IMethodSymbol eventHandlerCandidate, EventType eventType, 
																	 EventTargetKind eventTargetKind, int parametersCount)
		{
			if (!IsValidNumberOfParametersForClassicGraphEventHandler(eventHandlerCandidate, eventType, eventTargetKind, parametersCount))
				return false;

			const char underscore = '_';

			if (eventHandlerCandidate.Name[0] == underscore || eventHandlerCandidate.Name[^1] == underscore)
				return false;

			int underscoresCount = eventHandlerCandidate.Name.Count(c => c == underscore);
			return eventTargetKind switch
			{
				EventTargetKind.Row   => underscoresCount == 1,
				EventTargetKind.Field => underscoresCount == 2,
				_ 					  => false,
			};
		}

		private static bool IsValidNumberOfParametersForClassicGraphEventHandler(IMethodSymbol eventHandlerCandidate, EventType eventType,
																				 EventTargetKind eventTargetKind, int parametersCount)
		{
			if (eventTargetKind == EventTargetKind.Field)
			{
				return eventType == EventType.CacheAttached
					? parametersCount is (1 or 2)
					: parametersCount >= 2;                 // 2 or 3 parameters
			}
			else
				return parametersCount >= 2;                // 2 or 3 parameters
		}

		private static bool IsValidGenericGraphEventHandlerSignature(IMethodSymbol eventHandlerCandidate, int parametersCount) =>
			parametersCount is (1 or 2);
	}
}