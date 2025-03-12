#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel : ISemanticModel
	{
		private readonly CancellationToken _cancellation;

		public PXContext PXContext => BaseGraphModel.PXContext;

		public PXGraphSemanticModel BaseGraphModel { get; }

		#region Base Model
		/// <inheritdoc cref="PXGraphSemanticModel.ModelCreationOptions"/>
		public GraphSemanticModelCreationOptions ModelCreationOptions => BaseGraphModel.ModelCreationOptions;

		/// <inheritdoc cref="PXGraphSemanticModel.IsProcessing"/>
		public bool IsProcessing => BaseGraphModel.IsProcessing;

		/// <inheritdoc cref="PXGraphSemanticModel.HasPXProtectedAccess"/>
		public bool HasPXProtectedAccess => BaseGraphModel.HasPXProtectedAccess;

		/// <inheritdoc cref="PXGraphSemanticModel.ConfiguresWorkflow"/>
		public bool ConfiguresWorkflow => BaseGraphModel.ConfiguresWorkflow;

		/// <inheritdoc cref="PXGraphSemanticModel.GraphType"/>
		public GraphType GraphType => BaseGraphModel.GraphType;

		/// <inheritdoc cref="PXGraphSemanticModel.GraphOrGraphExtInfo"/>
		public GraphOrGraphExtInfoBase GraphOrGraphExtInfo => BaseGraphModel.GraphOrGraphExtInfo;

		public string Name => BaseGraphModel.Name;

		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => GraphOrGraphExtInfo.IsInMetadata;

		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => GraphOrGraphExtInfo.IsInSource;

		/// <inheritdoc cref="PXGraphSemanticModel.Node"/>
		public ClassDeclarationSyntax? Node => BaseGraphModel.Node;

		/// <inheritdoc cref="PXGraphSemanticModel.Symbol"/>
		public INamedTypeSymbol Symbol => BaseGraphModel.Symbol;

		/// <inheritdoc cref="PXGraphSemanticModel.GraphSymbol"/>
		public ITypeSymbol? GraphSymbol => BaseGraphModel.GraphSymbol;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclarationOrder"/>
		public int DeclarationOrder => BaseGraphModel.DeclarationOrder;

		/// <inheritdoc cref="PXGraphSemanticModel.StaticConstructors"/>
		public ImmutableArray<StaticConstructorInfo> StaticConstructors => BaseGraphModel.StaticConstructors;

		/// <inheritdoc cref="PXGraphSemanticModel.Initializers"/>
		public ImmutableArray<GraphInitializerInfo> Initializers => BaseGraphModel.Initializers;

		/// <inheritdoc cref="PXGraphSemanticModel.ViewsByNames"/>
		public ImmutableDictionary<string, DataViewInfo> ViewsByNames => BaseGraphModel.ViewsByNames;

		/// <inheritdoc cref="PXGraphSemanticModel.Views"/>
		public IEnumerable<DataViewInfo> Views => BaseGraphModel.Views;

		/// <inheritdoc cref="PXGraphSemanticModel.ViewDelegatesByNames"/>
		public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames => BaseGraphModel.ViewDelegatesByNames;

		/// <inheritdoc cref="PXGraphSemanticModel.ViewDelegates"/>
		public IEnumerable<DataViewDelegateInfo> ViewDelegates => BaseGraphModel.ViewDelegates;

		/// <inheritdoc cref="PXGraphSemanticModel.ActionsByNames"/>
		public ImmutableDictionary<string, ActionInfo> ActionsByNames => BaseGraphModel.ActionsByNames;

		/// <inheritdoc cref="PXGraphSemanticModel.Actions"/>
		public IEnumerable<ActionInfo> Actions => BaseGraphModel.Actions;

		/// <inheritdoc cref="PXGraphSemanticModel.ActionHandlersByNames"/>
		public ImmutableDictionary<string, ActionHandlerInfo> ActionHandlersByNames => BaseGraphModel.ActionHandlersByNames;

		/// <inheritdoc cref="PXGraphSemanticModel.ActionHandlers"/>
		public IEnumerable<ActionHandlerInfo> ActionHandlers => BaseGraphModel.ActionHandlers;

		/// <inheritdoc cref="PXGraphSemanticModel.IsActiveMethodInfo"/>
		public IsActiveMethodInfo? IsActiveMethodInfo => BaseGraphModel.IsActiveMethodInfo;

		/// <inheritdoc cref="PXGraphSemanticModel.IsActiveForGraphMethodInfo"/>
		public IsActiveForGraphMethodInfo? IsActiveForGraphMethodInfo => BaseGraphModel.IsActiveForGraphMethodInfo;

		/// <inheritdoc cref="PXGraphSemanticModel.ConfigureMethodOverride"/>
		public ConfigureMethodInfo? ConfigureMethodOverride => BaseGraphModel.ConfigureMethodOverride;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredConfigureMethodOverride"/>
		public ConfigureMethodInfo? DeclaredConfigureMethodOverride => BaseGraphModel.DeclaredConfigureMethodOverride;

		/// <inheritdoc cref="PXGraphSemanticModel.InitializeMethodInfo"/>
		public InitializeMethodInfo? InitializeMethodInfo => BaseGraphModel.InitializeMethodInfo;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredInitializeMethodInfo"/>
		public InitializeMethodInfo? DeclaredInitializeMethodInfo => BaseGraphModel.DeclaredInitializeMethodInfo;

		/// <inheritdoc cref="PXGraphSemanticModel.PXOverrides"/>
		public ImmutableArray<PXOverrideInfo> PXOverrides => BaseGraphModel.PXOverrides;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredActions"/>
		public IEnumerable<ActionInfo> DeclaredActions => BaseGraphModel.DeclaredActions;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredActionHandlers"/>
		public IEnumerable<ActionHandlerInfo> DeclaredActionHandlers => BaseGraphModel.DeclaredActionHandlers;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredViews"/>
		public IEnumerable<DataViewInfo> DeclaredViews => BaseGraphModel.DeclaredViews;

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredViewDelegates"/>
		public IEnumerable<DataViewDelegateInfo> DeclaredViewDelegates => BaseGraphModel.DeclaredViewDelegates;

		/// <inheritdoc cref="PXGraphSemanticModel.Attributes"/>
		public IEnumerable<GraphAttributeInfo> Attributes => BaseGraphModel.Attributes;
		#endregion

		#region Events
		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectingByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowSelectingEvents => RowSelectingByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowSelectedByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowSelectedEvents => RowSelectedByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertingByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowInsertingEvents => RowInsertingByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowInsertedByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowInsertedEvents => RowInsertedByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatingByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowUpdatingEvents => RowUpdatingByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowUpdatedByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowUpdatedEvents => RowUpdatedByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletingByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowDeletingEvents => RowDeletingByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowDeletedByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowDeletedEvents => RowDeletedByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistingByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowPersistingEvents => RowPersistingByName.Values;

		public ImmutableDictionary<string, GraphRowEventHandlerInfo> RowPersistedByName { get; }
		public IEnumerable<GraphRowEventHandlerInfo> RowPersistedEvents => RowPersistedByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldSelectingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> FieldSelectingEvents => FieldSelectingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldDefaultingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> FieldDefaultingEvents => FieldDefaultingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldVerifyingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> FieldVerifyingEvents => FieldVerifyingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatingEvents => FieldUpdatingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> FieldUpdatedByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> FieldUpdatedEvents => FieldUpdatedByName.Values;

		public ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> CacheAttachedByName { get; }
		public IEnumerable<GraphCacheAttachedEventHandlerInfo> CacheAttachedEvents => CacheAttachedByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> CommandPreparingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> CommandPreparingEvents => CommandPreparingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventHandlerInfo> ExceptionHandlingByName { get; }
		public IEnumerable<GraphFieldEventHandlerInfo> ExceptionHandlingEvents => ExceptionHandlingByName.Values;
		#endregion

		private PXGraphEventSemanticModel(PXGraphSemanticModel baseGraphModel, CancellationToken cancellation = default)
		{
			_cancellation = cancellation;
			BaseGraphModel = baseGraphModel;

			var eventsCollector = new EventsCollector(this, PXContext);
			eventsCollector.CollectGraphEvents(_cancellation);

			RowSelectingByName = GetRowEvents(eventsCollector, EventType.RowSelecting);
			RowSelectedByName  = GetRowEvents(eventsCollector, EventType.RowSelected);

			RowInsertingByName = GetRowEvents(eventsCollector, EventType.RowInserting);
			RowInsertedByName  = GetRowEvents(eventsCollector, EventType.RowInserted);

			RowUpdatingByName = GetRowEvents(eventsCollector, EventType.RowUpdating);
			RowUpdatedByName  = GetRowEvents(eventsCollector, EventType.RowUpdated);

			RowDeletingByName = GetRowEvents(eventsCollector, EventType.RowDeleting);
			RowDeletedByName  = GetRowEvents(eventsCollector, EventType.RowDeleted);

			RowPersistingByName = GetRowEvents(eventsCollector, EventType.RowPersisting);
			RowPersistedByName  = GetRowEvents(eventsCollector, EventType.RowPersisted);

			CacheAttachedByName = GetCacheAttachedEvents(eventsCollector);

			FieldSelectingByName  = GetFieldEvents(eventsCollector, EventType.FieldSelecting);
			FieldDefaultingByName = GetFieldEvents(eventsCollector, EventType.FieldDefaulting);
			FieldVerifyingByName  = GetFieldEvents(eventsCollector, EventType.FieldVerifying);
			FieldUpdatingByName   = GetFieldEvents(eventsCollector, EventType.FieldUpdating);
			FieldUpdatedByName 	  = GetFieldEvents(eventsCollector, EventType.FieldUpdated);

			CommandPreparingByName 	= GetFieldEvents(eventsCollector, EventType.CommandPreparing);
			ExceptionHandlingByName = GetFieldEvents(eventsCollector, EventType.ExceptionHandling);
		}

		public static PXGraphEventSemanticModel EnrichGraphModelWithEvents(PXGraphSemanticModel baseGraphModel, 
																		   CancellationToken cancellationToken = default) =>
			new PXGraphEventSemanticModel(baseGraphModel.CheckIfNull(), 
										  cancellationToken);

		public static IEnumerable<PXGraphEventSemanticModel> InferModels(PXContext pxContext, INamedTypeSymbol typeSymbol, 
																		 GraphSemanticModelCreationOptions modelCreationOptions,
																		 int? declarationOrder = null, CancellationToken cancellation = default)
		{	
			var baseGraphModels = PXGraphSemanticModel.InferModels(pxContext, typeSymbol, modelCreationOptions, declarationOrder, cancellation);
			var eventsGraphModels = baseGraphModels.Select(graph => new PXGraphEventSemanticModel(graph, cancellation))
												   .ToList();
			return eventsGraphModels;
		}

		public IEnumerable<GraphEventHandlerInfoBase> GetEventsByEventType(EventType eventType) => eventType switch
		{
			EventType.RowSelecting 		=> RowSelectingEvents,
			EventType.RowSelected 		=> RowSelectedEvents,
			EventType.RowInserting 		=> RowInsertingEvents,
			EventType.RowInserted 		=> RowInsertedEvents,
			EventType.RowUpdating 		=> RowUpdatingEvents,
			EventType.RowUpdated 		=> RowUpdatedEvents,
			EventType.RowDeleting 		=> RowDeletingEvents,
			EventType.RowDeleted 		=> RowDeletedEvents,
			EventType.RowPersisting 	=> RowPersistingEvents,
			EventType.RowPersisted 		=> RowPersistedEvents,

			EventType.FieldSelecting 	=> FieldSelectingEvents,
			EventType.FieldDefaulting 	=> FieldDefaultingEvents,
			EventType.FieldVerifying 	=> FieldVerifyingEvents,
			EventType.FieldUpdating 	=> FieldUpdatingEvents,
			EventType.FieldUpdated 		=> FieldUpdatedEvents,

			EventType.CacheAttached 	=> CacheAttachedEvents,
			EventType.CommandPreparing 	=> CommandPreparingEvents,
			EventType.ExceptionHandling => ExceptionHandlingEvents,
			EventType.None 				=> [],
			_ 							=> throw new NotSupportedException($"Event type {eventType} is not supported")
		};

		public IEnumerable<GraphEventHandlerInfoBase> GetAllEvents()
		{
			IEnumerable<GraphEventHandlerInfoBase>? allEvents = RowSelectingByName.Values;

			AppendRowEvents(RowSelectedByName);
			AppendRowEvents(RowInsertingByName);
			AppendRowEvents(RowInsertedByName);
			AppendRowEvents(RowUpdatingByName);
			AppendRowEvents(RowUpdatedByName);
			AppendRowEvents(RowDeletingByName);
			AppendRowEvents(RowDeletedByName);
			AppendRowEvents(RowPersistingByName);
			AppendRowEvents(RowPersistedByName);

			AppendFieldEvents(FieldSelectingByName);
			AppendFieldEvents(FieldDefaultingByName);
			AppendFieldEvents(FieldVerifyingByName);
			AppendFieldEvents(FieldUpdatingByName);
			AppendFieldEvents(FieldUpdatedByName);

			if (CacheAttachedByName.Count > 0)
				allEvents = allEvents.Concat(CacheAttachedByName.Values);

			AppendFieldEvents(CommandPreparingByName);
			AppendFieldEvents(ExceptionHandlingByName);

			return allEvents;

			//------------------------------------Local Function----------------------------------------------
			void AppendRowEvents(ImmutableDictionary<string, GraphRowEventHandlerInfo> rowEvents)
			{
				if (rowEvents.Count > 0)
					allEvents = allEvents.Concat(rowEvents.Values);
			}

			void AppendFieldEvents(ImmutableDictionary<string, GraphFieldEventHandlerInfo> fieldEvents)
			{
				if (fieldEvents.Count > 0)
					allEvents = allEvents.Concat(fieldEvents.Values);
			}
		}

		private ImmutableDictionary<string, GraphRowEventHandlerInfo> GetRowEvents(EventsCollector eventsCollector, EventType eventType)
		{
			OverridableItemsCollection<GraphRowEventHandlerInfo>? rawCollection = eventsCollector.GetRowEvents(eventType);
			return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphRowEventHandlerInfo>.Empty;
		}

		private ImmutableDictionary<string, GraphFieldEventHandlerInfo> GetFieldEvents(EventsCollector eventsCollector, EventType eventType)
		{
			OverridableItemsCollection<GraphFieldEventHandlerInfo>? rawCollection = eventsCollector.GetFieldEvents(eventType);
			return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphFieldEventHandlerInfo>.Empty;
		}

		private ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo> GetCacheAttachedEvents(EventsCollector eventsCollector)
		{
			OverridableItemsCollection<GraphCacheAttachedEventHandlerInfo>? rawCollection = eventsCollector.CacheAttachedEvents;
			return rawCollection?.ToImmutableDictionary() ?? ImmutableDictionary<string, GraphCacheAttachedEventHandlerInfo>.Empty;
		}
	}
}
