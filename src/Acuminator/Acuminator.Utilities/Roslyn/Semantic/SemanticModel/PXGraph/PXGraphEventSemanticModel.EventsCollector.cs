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
		private class EventsCollector
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

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
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

				var methods = GetAllGraphMethodsFromBaseToDerived();
				int declarationOrder = 0;

				foreach (IMethodSymbol method in methods)
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

			private IEnumerable<IMethodSymbol> GetAllGraphMethodsFromBaseToDerived()
			{
				var graphModel = _graphEventSemanticModel.BaseGraphModel;
				IEnumerable<ITypeSymbol>? baseTypes = graphModel.GraphSymbol
															   ?.GetGraphWithBaseTypes()
																.Reverse();

				if (graphModel.GraphType == GraphType.PXGraphExtension)
				{
					baseTypes = baseTypes?.Concat(
											graphModel.Symbol.GetGraphExtensionWithBaseExtensions(_pxContext,
																								  SortDirection.Ascending,
																								  includeGraph: false));
				}

				return baseTypes?.SelectMany(t => t.GetMethods()) ?? [];
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
						CacheAttachedEvents.Add(cacheAttachedEventInfo);
						return;
				}
			}

			private void AddEventHandlerInfo<TEventHandlerInfo>(TEventHandlerInfo eventHandlerInfo, 
																Dictionary<EventType, OverridableItemsCollection<TEventHandlerInfo>> eventHandlersByEventType)
			where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
			{
				if (!eventHandlersByEventType.TryGetValue(eventHandlerInfo.EventType, out var eventHandlers))
					return;

				eventHandlers.Add(eventHandlerInfo);
			}
		}
	}
}
