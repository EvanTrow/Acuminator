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

				var graphHierarchyFromBaseGraphToDerivedExtension = GetGraphAndGraphExtensionTypesHierarchyInEventHandlersCollectionOrder(cancellation);

				if (graphHierarchyFromBaseGraphToDerivedExtension.IsNullOrEmpty())
					return;

				int declarationOrder = 0;

				foreach (var (graphOrExtType, isGraph) in graphHierarchyFromBaseGraphToDerivedExtension)
				{
					cancellation.ThrowIfCancellationRequested();
					var declaredMethods = graphOrExtType.GetMethods();

					foreach (var method in declaredMethods)
					{
						cancellation.ThrowIfCancellationRequested();

						var handlerInfo = GraphEventsRecognition.TryRecognizeEventHandler(method, _pxContext, declarationOrder, cancellation);

						if (handlerInfo != null)
						{
							AddEventHandlerInfo(handlerInfo, isDeclaredInGraph: isGraph);
							declarationOrder++;
						}
					}
				}
			}

			/// <summary>
			/// Gets graph and graph extensions type hierarchy in the order of event handlers collection.<br/>
			/// The order of types during the collection of event handlers is:<br/>
			/// Derived Graph | Base Graph | Extension 1-st Lvl from derived type to base | Extension 2-nd Lvl from derived type to base | ...
			/// </summary>
			/// <param name="cancellation">Cancellation</param>
			/// <returns/>
			private List<(ITypeSymbol Type, bool IsGraph)>? GetGraphAndGraphExtensionTypesHierarchyInEventHandlersCollectionOrder(
																											CancellationToken cancellation)
			{				
				if (_graphEventSemanticModel.GraphSymbol == null)
					return null;

				var graphTypesFromDerivedToBase = _graphEventSemanticModel.GraphSymbol.GetGraphWithBaseTypes()
																					  .Select(graphType => (Type: graphType, IsGraph: true));
				var typesHierarchy = graphTypesFromDerivedToBase;

				cancellation.ThrowIfCancellationRequested();

				if (_graphEventSemanticModel.GraphType == GraphType.PXGraphExtension)
				{
					var extensionTypes = GetGraphExtensionTypesInEventHandlersCollectionOrder(cancellation);

					if (extensionTypes.Count > 0)
					{
						var extensionTypesWithIndicator = extensionTypes.Select(extensionType => (Type: extensionType, IsGraph: false));
						typesHierarchy = typesHierarchy.Concat(extensionTypesWithIndicator);
					}
				}

				return typesHierarchy.ToList();
			}

			/// <summary>
			/// Gets graph extensions type hierarchy in the order of event handlers collection.<br/>
			/// The order of types during the collection of event handlers is:<br/>
			/// Derived Graph | Base Graph | Extension 1-st Lvl from derived type to base | Extension 2-nd Lvl from derived type to base | ...
			/// </summary>
			/// <param name="cancellation">Cancellation</param>
			/// <returns/>
			private IReadOnlyCollection<ITypeSymbol> GetGraphExtensionTypesInEventHandlersCollectionOrder(CancellationToken cancellation)
			{
				var extensionBaseType = _graphEventSemanticModel.Symbol.GetBaseTypesAndThis()
																	   .FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
				if (extensionBaseType == null)
					return [];

				var graphType = extensionBaseType.TypeArguments.LastOrDefault();

				if (graphType == null)
					return [];

				int graphIndex = extensionBaseType.TypeArguments.Length - 1;
				IEnumerable<ITypeSymbol> allExtensions = [];

				for (int i = graphIndex - 1; i >= 0; i--)	// going from the lowest to the highest level of base extensions in extensions chaining
				{
					var baseExtension = extensionBaseType.TypeArguments[i];

					// early exit in case of messed up graph extension declaration
					if (!baseExtension.IsPXGraphExtension(_pxContext))
						return [];

					// The order of graph extension's base types during the extensions loading is actually impossible to predict
					// It is sorted by full name, then manually, and then by the level of chained extension from base to derived levels
					// Here Acuminator will just impose some natural ordering from derived extension type to the base extension type
					var extensionWithBaseTypes = baseExtension.GetGraphExtensionWithBaseTypes();
					allExtensions = allExtensions.Concat(extensionWithBaseTypes);
				}

				var highestLevelGraphExtensionTypes = _graphEventSemanticModel.Symbol.GetGraphExtensionWithBaseTypes();
				allExtensions = allExtensions.Concat(highestLevelGraphExtensionTypes);

				// We need to remove duplicates from the collection of extensions for a theorethical degenerate case 
				// when the graph extension chains two or more base extensions that are derived from each other with .Net inheritance
				allExtensions = allExtensions.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default);

				int estimatedExtensionsCount = Math.Max(4, extensionBaseType.TypeArguments.Length * 2);
				return allExtensions.ToList(estimatedExtensionsCount);
			}

			private void AddEventHandlerInfo(GraphEventHandlerInfoBase eventHandlerInfo, bool isDeclaredInGraph)
			{
				switch (eventHandlerInfo)
				{
					case GraphRowEventHandlerInfo rowEventInfo 
					when !rowEventInfo.DacName.IsNullOrEmpty():
						AddEventHandlerInfo(rowEventInfo, isDeclaredInGraph, _rowEvents);
						return;

					case GraphFieldEventHandlerInfo fieldEventInfo
					when !fieldEventInfo.DacName.IsNullOrEmpty() && !fieldEventInfo.DacFieldName.IsNullOrEmpty():
						AddEventHandlerInfo(fieldEventInfo, isDeclaredInGraph, _fieldEvents);
						return;
					
					case GraphCacheAttachedEventHandlerInfo cacheAttachedEventInfo
					when !cacheAttachedEventInfo.DacName.IsNullOrEmpty() && !cacheAttachedEventInfo.DacFieldName.IsNullOrEmpty():
						AddEventHandlerInfo(cacheAttachedEventInfo, isDeclaredInGraph, CacheAttachedEventHandlers);
						return;
				}
			}

			private static void AddEventHandlerInfo<TEventHandlerInfo>(TEventHandlerInfo eventHandlerInfo, bool isDeclaredInGraph,
																Dictionary<EventType, OverridableItemsCollection<TEventHandlerInfo>> eventHandlersByEventType)
			where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
			{
				if (!eventHandlersByEventType.TryGetValue(eventHandlerInfo.EventType, out var eventHandlers))
					return;

				AddEventHandlerInfo(eventHandlerInfo, isDeclaredInGraph, eventHandlers);
			}

			private static void AddEventHandlerInfo<TEventHandlerInfo>(TEventHandlerInfo eventHandlerInfo, bool isDeclaredInGraph,
																		OverridableItemsCollection<TEventHandlerInfo> eventHandlersCollection)
			where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
			{
				if (!eventHandlersCollection.TryGetValue(eventHandlerInfo.Name, out TEventHandlerInfo existingHandlerInfo))
				{
					eventHandlersCollection.Add(eventHandlerInfo.Name, eventHandlerInfo);
					return;
				}

				if (ReferenceEquals(eventHandlerInfo, existingHandlerInfo))
					return;

				// The order of types during hte collection of event handlers is:
				// Derived Graph | Base Graph | Extension 1-st Lvl and its base types | Extension 2-nd Lvl and its base types | Extension 3-rd Lvl and its base types

				if (eventHandlerInfo.CollectionMode == EventCollectionMode.AddedToBeginning)
				{

				}
				else 
				{
					// The collection for events added to the end (-ed events + row selecting) places them from the graph to the most derived graph extension.
					// The exception is graph inheritance which breaks this principle - event handlers from the Derived Graph are executed before event handlers 
					// from the Base Graph:
					// Derived Graph | Base Graph | Ext 1-st lvl | Ext 2-nd lvl | Ext 3-rd lvl |

					if (isDeclaredInGraph)
					{
						// HACK the assignment below is required due to the issue with C# compiler, see details here:
						// https://developercommunity.visualstudio.com/t/False-compiler-Error-CS0229-with-intende/10560802
						IWriteableBaseItem<TEventHandlerInfo> existingEventHandlerFromBaseGraph = existingHandlerInfo;

						// For the graph inheritance case the derived event handler wil be collected and executed before 
						existingEventHandlerFromBaseGraph.Base = eventHandlerInfo;
					}
					else
					{
						// HACK the assignment below is required due to the issue with C# compiler, see details here:
						// https://developercommunity.visualstudio.com/t/False-compiler-Error-CS0229-with-intende/10560802
						IWriteableBaseItem<TEventHandlerInfo> eventHandlerInfoFromExtension = eventHandlerInfo;
					}

					

					eventHandlerInfoWriteableBaseItem.Base = existingHandlerInfo;
					eventHandlersCollection[eventHandlerInfo.Name] = eventHandlerInfo;
				}
			}
		}
	}
}
