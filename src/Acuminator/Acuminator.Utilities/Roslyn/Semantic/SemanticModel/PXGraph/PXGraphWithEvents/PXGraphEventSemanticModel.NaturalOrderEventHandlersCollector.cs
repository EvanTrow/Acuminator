using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel : ISemanticModel
	{
		/// <summary>
		/// An event handlers collector that collects event handlers in the natural intuitive order - from base types to derived types.<br/>
		/// It collects handlers from base graph to the most derived graph extension.
		/// </summary>
		/// <remarks>
		/// Warning - the natural collection order of event handlers is different from the collection order of event handlers of the Acumatica runtime.<br/>
		/// The natural collection order is used for simplicity and intuitive understanding of the event handlers order.<br/>
		/// </remarks>
		private class NaturalOrderEventHandlersCollector
		{
			private readonly PXContext _pxContext;
			private readonly PXGraphEventSemanticModel _graphEventSemanticModel;

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphRowEventHandlerInfo>> _rowEvents = 
				new()
				{
					[EventType.RowSelecting]  = [],
					[EventType.RowSelected]   = [],

					[EventType.RowInserting]  = [],
					[EventType.RowInserted]   = [],

					[EventType.RowUpdating]   = [],
					[EventType.RowUpdated] 	  = [],

					[EventType.RowDeleting]   = [],
					[EventType.RowDeleted] 	  = [],

					[EventType.RowPersisting] = [],
					[EventType.RowPersisted]  = [],
				};

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphFieldEventHandlerInfo>> _fieldEvents =
				new()
				{
					[EventType.FieldSelecting] 	  = [],
					[EventType.FieldDefaulting]   = [],
					[EventType.FieldVerifying] 	  = [],
					[EventType.FieldUpdating] 	  = [],
					[EventType.FieldUpdated] 	  = [],

					[EventType.CommandPreparing]  = [],
					[EventType.ExceptionHandling] = [],
				};

			public OverridableItemsCollection<GraphCacheAttachedEventHandlerInfo> CacheAttachedEventHandlers { get; } = [];

			public NaturalOrderEventHandlersCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public OverridableItemsCollection<GraphRowEventHandlerInfo>? GetRowEventHandlers(EventType eventType) =>
				_rowEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphRowEventHandlerInfo> events)
					? events
					: null;

			public OverridableItemsCollection<GraphFieldEventHandlerInfo>? GetFieldEventHandlers(EventType eventType) =>
				_fieldEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphFieldEventHandlerInfo> events)
					? events
					: null;

			public void CollectGraphEventHandlers(CancellationToken cancellation)
			{
				cancellation.ThrowIfCancellationRequested();

				var graphHierarchyFromBaseGraphToDerivedExtension = _graphEventSemanticModel.GraphOrGraphExtInfo.ThisAndOverridenItems()
																												.Reverse();
				int declarationOrder = 0;

				foreach (GraphOrGraphExtInfoBase graphOrExtInfo in graphHierarchyFromBaseGraphToDerivedExtension)
				{
					cancellation.ThrowIfCancellationRequested();
					var declaredMethods = graphOrExtInfo.Symbol.GetMethods();

					foreach (var method in declaredMethods)
					{
						cancellation.ThrowIfCancellationRequested();

						var handlerInfo = GraphEventsRecognition.TryRecognizeEventHandler(method, _pxContext, declarationOrder, cancellation);

						if (handlerInfo != null)
						{
							AddEventHandlerInfo(handlerInfo);
							declarationOrder++;
						}
					}
				}
			}

			private void AddEventHandlerInfo(GraphEventHandlerInfoBase eventHandlerInfo)
			{
				switch (eventHandlerInfo)
				{
					case GraphRowEventHandlerInfo rowEventInfo 
					when !rowEventInfo.DacName.IsNullOrEmpty():
						AddEventHandlerInfo(rowEventInfo, _rowEvents);
						return;

					case GraphFieldEventHandlerInfo fieldEventInfo
					when !fieldEventInfo.DacName.IsNullOrEmpty() && !fieldEventInfo.DacFieldName.IsNullOrEmpty():
						AddEventHandlerInfo(fieldEventInfo, _fieldEvents);
						return;
					
					case GraphCacheAttachedEventHandlerInfo cacheAttachedEventInfo
					when !cacheAttachedEventInfo.DacName.IsNullOrEmpty() && !cacheAttachedEventInfo.DacFieldName.IsNullOrEmpty():
						AddEventHandlerInfo(cacheAttachedEventInfo, CacheAttachedEventHandlers);
						return;
				}
			}

			private static void AddEventHandlerInfo<TEventHandlerInfo>(TEventHandlerInfo eventHandlerInfo,
																Dictionary<EventType, OverridableItemsCollection<TEventHandlerInfo>> eventHandlersByEventType)
			where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
			{
				if (!eventHandlersByEventType.TryGetValue(eventHandlerInfo.EventType, out var eventHandlers))
					return;

				AddEventHandlerInfo(eventHandlerInfo, eventHandlers);
			}

			private static void AddEventHandlerInfo<TEventHandlerInfo>(TEventHandlerInfo eventHandlerInfo,
																		OverridableItemsCollection<TEventHandlerInfo> eventHandlersCollection)
			where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
			{
				eventHandlersCollection.Add(eventHandlerInfo);
			}
		}
	}
}
