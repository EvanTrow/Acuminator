using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Shared;

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

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredInitializers"/>
		public ImmutableArray<GraphInitializerInfo> DeclaredInitializers => BaseGraphModel.DeclaredInitializers;

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

		/// <inheritdoc cref="PXGraphSemanticModel.DeclaredPXOverrides"/>
		public ImmutableArray<PXOverrideInfo> DeclaredPXOverrides => BaseGraphModel.DeclaredPXOverrides;

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

		#region Event Handlers Storage
		/// <summary>
		/// The storage with all event handlers override chains ordered in the natural collection order.<br/>
		/// This means, that overrides of the same event handler are ordered by their containing types:<br/>
		/// from the handler declared in the most derived graph or graph extension type to event handlers declared in ancestor types.
		/// </summary>
		/// <remarks>
		/// Warning: Some event handlers may be declared not in the <see cref="Symbol"/> type but in its base types.
		/// </remarks>
		public IGraphEventHandlerOverridesChainsStorage AllEventHandlerOverridesChains { get; }

		/// <summary>
		/// The storage with all event handlers declared in <see cref="Symbol"/>.<br/>
		/// For each event type, DAC and optionally DAC field, it stores an array of event handlers declared in the current <see cref="Symbol"/> type.
		/// </summary>
		/// <remarks>
		/// Warning: Event handlers declared in base types may be missing in this collection unless they are overridden in the <see cref="Symbol"/> type.<br/>
		/// Use <see cref="AllEventHandlerOverridesChains"/> to get all event handlers.
		/// </remarks>
		public IDeclaredGraphEventHandlerStorage DeclaredEventHandlers { get; }
		#endregion

		private PXGraphEventSemanticModel(PXGraphSemanticModel baseGraphModel, CancellationToken cancellation = default)
		{
			_cancellation = cancellation;
			BaseGraphModel = baseGraphModel.CheckIfNull();

			var eventsCollector = new NaturalOrderEventHandlersCollector(this, PXContext);
			eventsCollector.CollectGraphEventHandlers(_cancellation);

			AllEventHandlerOverridesChains = AllGraphEventHandlerOverridesChainsStorage.FromCollected(eventsCollector);
			DeclaredEventHandlers = DeclaredGraphEventHandlerStorage.FromOverridesChainsStorageInDerivedToBaseOrder(Symbol, AllEventHandlerOverridesChains);
		}

		public static PXGraphEventSemanticModel EnrichGraphModelWithEvents(PXGraphSemanticModel baseGraphModel, 
																		   CancellationToken cancellationToken = default) =>
			new PXGraphEventSemanticModel(baseGraphModel.CheckIfNull(), cancellationToken);

		public static PXGraphEventSemanticModel? InferModel(PXContext pxContext, GraphOrGraphExtInfoBase graphOrGraphExtInferredInfo,
															GraphSemanticModelCreationOptions modelCreationOptions,
															int? declarationOrder = null, CancellationToken cancellation = default)
		{	
			var baseGraphModel = PXGraphSemanticModel.InferModel(pxContext, graphOrGraphExtInferredInfo, modelCreationOptions, 
																 declarationOrder, cancellation);
			return baseGraphModel != null 
				? new PXGraphEventSemanticModel(baseGraphModel, cancellation)
				: null;
		}
	}
}
