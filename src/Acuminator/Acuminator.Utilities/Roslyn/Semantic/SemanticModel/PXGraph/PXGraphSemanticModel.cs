#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class PXGraphSemanticModel : ISemanticModel
	{
		protected readonly CancellationToken _cancellation;

		public PXContext PXContext { get; }

		public GraphSemanticModelCreationOptions ModelCreationOptions { get; }

		public bool IsProcessing { get; private set; }

		public GraphType GraphType { get; }

		public GraphOrGraphExtInfoBase GraphOrGraphExtInfo { get; }

		public string Name => GraphOrGraphExtInfo.Name;

		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => GraphOrGraphExtInfo.IsInMetadata;

		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => GraphOrGraphExtInfo.IsInSource;

		public INamedTypeSymbol Symbol => GraphOrGraphExtInfo.Symbol;

		public ClassDeclarationSyntax? Node => GraphOrGraphExtInfo.Node;

		public int DeclarationOrder => GraphOrGraphExtInfo.DeclarationOrder;

		/// <summary>
		/// The graph symbol. For a graph, the value is the same as <see cref="Symbol"/>. 
		/// For a graph extension, the value is the symbol of the extension's base graph.
		/// </summary>
		public ITypeSymbol? GraphSymbol { get; }

		public ImmutableArray<StaticConstructorInfo> StaticConstructors { get; }

		/// <summary>
		/// Gets or sets the initializers.
		/// </summary>
		/// <remarks>
		/// By initializers Acuminator understands special code elements of graph or graph extension that configure graph's initial state.<br/>
		/// Currently, initalizers consists of:
		/// <list type="bullet">
		/// <item>Graph and graph extension constructors.</item>
		/// <item><c>Initialize</c> method override of a graph extension.</item>
		/// <item><c>Initialize</c> method of a graph that implements <c>PX.Data.DependencyInjection.IGraphWithInitialization</c> interface.</item>
		/// <item><c>Configure</c> method override of a graph or graph extension that configure screen workflow.</item>
		/// </list>
		/// </remarks>
		/// <value>
		/// The initializers.
		/// </value>
		public ImmutableArray<GraphInitializerInfo> DeclaredInitializers { get; private set; }

		public ImmutableDictionary<string, DataViewInfo> ViewsByNames { get; }
		public IEnumerable<DataViewInfo> Views => ViewsByNames.Values;

		public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames { get; }
		public IEnumerable<DataViewDelegateInfo> ViewDelegates => ViewDelegatesByNames.Values;

		public ImmutableDictionary<string, ActionInfo> ActionsByNames { get; }
		public IEnumerable<ActionInfo> Actions => ActionsByNames.Values;

		public ImmutableDictionary<string, ActionHandlerInfo> ActionHandlersByNames { get; }
		public IEnumerable<ActionHandlerInfo> ActionHandlers => ActionHandlersByNames.Values;

		public ImmutableArray<PXOverrideInfo> PXOverrides { get; }

		/// <summary>
		/// Actions which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionInfo> DeclaredActions => 
			Actions.Where(action => action.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Action handlers which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionHandlerInfo> DeclaredActionHandlers =>
			ActionHandlers.Where(handler => handler.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Views which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<DataViewInfo> DeclaredViews =>
			Views.Where(view => view.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// View delegates which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<DataViewDelegateInfo> DeclaredViewDelegates =>
			ViewDelegates.Where(viewDelegate => viewDelegate.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Information about the IsActive method of the graph extensions. 
		/// The value can be <c>null</c>. The value is always <c>null</c> for a graph.
		/// </summary>
		/// <value>
		/// Information about the IsActive method.
		/// </value>
		public IsActiveMethodInfo? IsActiveMethodInfo { get; }

		/// <summary>
		/// Gets the info about IsActiveForGraph&lt;TGraph&gt; method for graph extensions. Can be <c>null</c>. Always <c>null</c> for graphs.
		/// </summary>
		/// <value>
		/// The info about IsActiveForGraph&lt;TGraph&gt; method.
		/// </value>
		public IsActiveForGraphMethodInfo? IsActiveForGraphMethodInfo { get; }

		/// <summary>
		/// Information about the graph's or the graph extension's Configure method override. The override can be declared in base types.
		/// </summary>
		public ConfigureMethodInfo? ConfigureMethodOverride { get; }

		/// <summary>
		/// Information about the Configure method override declared in this type. <see langword="null"/> if the method is not declared in this type.
		/// </summary>
		public ConfigureMethodInfo? DeclaredConfigureMethodOverride =>
			ConfigureMethodOverride != null && ConfigureMethodOverride.Symbol.IsDeclaredInType(Symbol)
				? ConfigureMethodOverride
				: null;

		/// <summary>
		/// An indicator of whether the graph or the graph extension configures a workflow.
		/// </summary>
		[MemberNotNullWhen(returnValue: true, nameof(ConfigureMethodOverride))]
		public bool ConfiguresWorkflow => ConfigureMethodOverride != null;

		/// <summary>
		/// Information about the graph's or the graph extension's Initialize method and its overrides. The method can be declared in base types.
		/// </summary>
		public InitializeMethodInfo? InitializeMethodInfo { get; }

		/// <summary>
		/// Information about the Initialize method declared in this type. <see langword="null"/> if the method is not declared in this type.
		/// </summary>
		public InitializeMethodInfo? DeclaredInitializeMethodInfo =>
			InitializeMethodInfo != null && InitializeMethodInfo.Symbol.IsDeclaredInType(Symbol)
				? InitializeMethodInfo
				: null;

		/// <summary>
		/// An indicator of whether the graph extension has the PXProtectedAccess attribute.
		/// </summary>
		public bool HasPXProtectedAccess { get; }

		/// <summary>
		/// The attributes declared on the graph or the graph extension.
		/// </summary>
		public ImmutableArray<GraphAttributeInfo> Attributes { get; }

		protected PXGraphSemanticModel(PXContext pxContext, GraphType graphType, GraphOrGraphExtInfoBase graphOrGraphExtInfo, ClassDeclarationSyntax? node,
										ITypeSymbol? graphSymbol, GraphSemanticModelCreationOptions modelCreationOptions, int declarationOrder,
										CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext 			= pxContext.CheckIfNull();
			GraphType 			= graphType;
			GraphOrGraphExtInfo = graphOrGraphExtInfo.CheckIfNull();
			GraphSymbol			= graphSymbol;

			_cancellation 		 = cancellation;
			ModelCreationOptions = modelCreationOptions;
			Attributes			 = GetGraphAttributes();

			StaticConstructors 	 = GraphOrGraphExtInfo.Symbol.GetStaticConstructors(_cancellation);
			ViewsByNames 		 = GetDataViews();
			ViewDelegatesByNames = GetDataViewDelegates();

			ActionsByNames 		  = GetActions();
			ActionHandlersByNames = GetActionHandlers();

			InitProcessingDelegatesInfo();

			ConfigureMethodOverride = ConfigureMethodInfo.GetConfigureMethodInfo(GraphOrGraphExtInfo.Symbol, GraphType, PXContext, _cancellation);
			InitializeMethodInfo	= InitializeMethodInfo.GetInitializeMethodInfo(GraphOrGraphExtInfo.Symbol, GraphType, PXContext, _cancellation);

			DeclaredInitializers 	   = GetDeclaredInitializers().ToImmutableArray();
			IsActiveMethodInfo 		   = GetIsActiveMethodInfo();
			IsActiveForGraphMethodInfo = GetIsActiveForGraphMethodInfo();
			
			PXOverrides = GetDeclaredPXOverrideInfos();
			HasPXProtectedAccess = IsPXProtectedAccessAttributeDeclared();
		}

		protected void InitProcessingDelegatesInfo()
		{
			if (!ModelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.CollectProcessingDelegates))
			{
				IsProcessing = Views.Any(v => v.IsProcessing);
				return;
			}

			var processingViewSymbols = Views.Where(v => v.IsProcessing)
											 .Select(v => v.Symbol)
											 .ToImmutableHashSet(SymbolEqualityComparer.Default);
			IsProcessing = processingViewSymbols.Count > 0;

			if (!IsProcessing)
			{
				return;
			}

			_cancellation.ThrowIfCancellationRequested();
			var declaringNodes = Symbol.DeclaringSyntaxReferences
									   .Select(r => r.GetSyntax(_cancellation));
			var walker = new ProcessingDelegatesWalker(PXContext, processingViewSymbols, _cancellation);

			foreach (var node in declaringNodes)
			{
				walker.Visit(node);
			}

			foreach (var (viewName, paramsDelegateInfo) in walker.ParametersDelegateListByView)
			{
				ViewsByNames[viewName].ParametersDelegates = paramsDelegateInfo.ToImmutableArray();
			}

			_cancellation.ThrowIfCancellationRequested();

			foreach (var (viewName, processDelegateInfo) in walker.ProcessDelegateListByView)
			{
				ViewsByNames[viewName].ProcessDelegates = processDelegateInfo.ToImmutableArray();
			}

			_cancellation.ThrowIfCancellationRequested();

			foreach (var (viewName, finalProcessDelegateInfo) in walker.FinallyProcessDelegateListByView)
			{
				ViewsByNames[viewName].FinallyProcessDelegates = finalProcessDelegateInfo.ToImmutableArray();
			}
		}

		protected ImmutableArray<GraphAttributeInfo> GetGraphAttributes()
		{
			var attributes = Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return ImmutableArray<GraphAttributeInfo>.Empty;

			var attributeInfos = attributes.Select((attributeData, relativeOrder) => new GraphAttributeInfo(PXContext, attributeData, relativeOrder));
			var builder = ImmutableArray.CreateBuilder<GraphAttributeInfo>(attributes.Length);
			builder.AddRange(attributeInfos);

			return builder.ToImmutable();
		}

		protected ImmutableDictionary<string, DataViewInfo> GetDataViews() =>
			GetInfos(() => Symbol.GetViewsWithSymbolsFromPXGraph(PXContext),
					 () => Symbol.GetViewsFromGraphExtensionAndBaseGraph(PXContext));

		protected ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates() =>
			GetInfos(() => Symbol.GetViewDelegatesFromGraph(ViewsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(ViewsByNames, PXContext, _cancellation));

		protected ImmutableDictionary<string, ActionInfo> GetActions() =>
			GetInfos(() => Symbol.GetActionSymbolsWithTypesFromGraph(PXContext),
					 () => Symbol.GetActionsFromGraphExtensionAndBaseGraph(PXContext));

		protected ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers() =>
			GetInfos(() => Symbol.GetActionHandlersFromGraph(ActionsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, PXContext, _cancellation));

		protected ImmutableDictionary<string, TInfo> GetInfos<TInfo>(Func<OverridableItemsCollection<TInfo>> graphInfosSelector,
																   Func<OverridableItemsCollection<TInfo>> graphExtInfosSelector)
		where TInfo : IOverridableItem<TInfo>
		{
			var infos = GraphType == GraphType.PXGraph
				? graphInfosSelector()
				: graphExtInfosSelector();

			return infos.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the declared initializers in this collection.
		/// </summary>
		/// <remarks>
		/// By initializer Acuminator understands special code elements of graph or graph extension that configure graph's initial state.<br/>
		/// Currently, initalizers consists of:
		/// <list type="bullet">
		/// <item>Graph and graph extension constructors.</item>
		/// <item><c>Initialize</c> method override of a graph extension.</item>
		/// <item><c>Initialize</c> method of a graph that implements <c>PX.Data.DependencyInjection.IGraphWithInitialization</c> interface.</item>
		/// <item><c>Configure</c> method override of a graph or graph extension that configure screen workflow.</item>
		/// </list>
		/// </remarks>
		/// <returns>
		/// The declared initializers in this collection.
		/// </returns>
		protected List<GraphInitializerInfo> GetDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			var constructors = Symbol.GetDeclaredInstanceConstructors(_cancellation)
									 .Select((ctr, order) => new GraphInitializerInfo(GraphInitializerType.InstanceConstructor, ctr.Node, ctr.Symbol, order));
			var initializerInfos = constructors.ToList(capacity: 4);
			int declarationOrder = initializerInfos.Count;

			if (DeclaredInitializeMethodInfo is { } declaredInitializeMethod)
			{
				var initializeMethodInfo = new GraphInitializerInfo(GraphInitializerType.InitializeMethod, declaredInitializeMethod.Node,
																	declaredInitializeMethod.Symbol, declarationOrder);
				declarationOrder++;
				initializerInfos.Add(initializeMethodInfo);
			}

			if (DeclaredConfigureMethodOverride is { } declaredConfigureMethod)
			{
				var configureMethodInfo = new GraphInitializerInfo(GraphInitializerType.ConfigureMethod, declaredConfigureMethod.Node,
																	declaredConfigureMethod.Symbol, declarationOrder);
				initializerInfos.Add(configureMethodInfo);
			}

			return initializerInfos;
		}

		/// <summary>
		/// Infer semantic model for a given <paramref name="graphOrGraphExtTypeSymbol"/>.
		/// If <paramref name="graphOrGraphExtTypeSymbol"/> is not a graph or graph extension, returns <see langword="null"/>.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="graphOrGraphExtTypeSymbol">The graph or graph extension type symbol.</param>
		/// <param name="modelCreationOptions">Options for controlling the semantic model creation.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// A semantic model for a given graph or graph extension <paramref name="graphOrGraphExtTypeSymbol"/>.<br/>
		/// If <paramref name="graphOrGraphExtTypeSymbol"/> is not graph or graph extension, then returns <see langword="null"/>.
		/// </returns>
		public static PXGraphSemanticModel? InferModel(PXContext pxContext, INamedTypeSymbol graphOrGraphExtTypeSymbol,
													   GraphSemanticModelCreationOptions modelCreationOptions,
													   int? customDeclarationOrder = null, CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull();
			graphOrGraphExtTypeSymbol.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			GraphType graphType;
			ITypeSymbol? graphSymbol;

			if (graphOrGraphExtTypeSymbol.IsPXGraph(pxContext))
			{
				graphType   = GraphType.PXGraph;
				graphSymbol = graphOrGraphExtTypeSymbol;
			}
			else if (graphOrGraphExtTypeSymbol.IsPXGraphExtension(pxContext))
			{
				graphType   = GraphType.PXGraphExtension;
				graphSymbol = graphOrGraphExtTypeSymbol.GetGraphFromGraphExtension(pxContext);
			}
			else
				return null;

			int declarationOrder = customDeclarationOrder ?? 0;
			var graphOrExtNode = graphOrGraphExtTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;

			GraphOrGraphExtInfoBase? graphOrGraphExtInfo;

			if (graphType == GraphType.PXGraph)
				graphOrGraphExtInfo = GraphInfo.Create(graphOrGraphExtTypeSymbol, graphOrExtNode, pxContext, declarationOrder, cancellation);
			else
			{
				(graphOrGraphExtInfo, bool hasCircularReferences) = 
					GraphExtensionInfo.Create(graphOrGraphExtTypeSymbol, graphOrExtNode, graphSymbol, pxContext, declarationOrder, cancellation);

				if (hasCircularReferences)
					return null;
			}

			if (graphOrGraphExtInfo == null)
				return null;

			return new PXGraphSemanticModel(pxContext, graphType, graphOrGraphExtInfo, graphOrExtNode, graphSymbol, modelCreationOptions, 
											declarationOrder, cancellation);
		}

		protected IsActiveMethodInfo? GetIsActiveMethodInfo() =>
			GraphType == GraphType.PXGraphExtension
				? IsActiveMethodInfo.GetIsActiveMethodInfo(Symbol, _cancellation)
				: null;

		protected IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo() =>
			GraphType == GraphType.PXGraphExtension
				? IsActiveForGraphMethodInfo.GetIsActiveForGraphMethodInfo(Symbol, _cancellation)
				: null;

		protected ImmutableArray<PXOverrideInfo> GetDeclaredPXOverrideInfos()
		{
			var pxOverrides = PXOverrideInfo.GetPXOverrides(Symbol, PXContext, _cancellation);
			return pxOverrides.ToImmutableArray();
		}

		protected bool IsPXProtectedAccessAttributeDeclared() =>
			GraphType == GraphType.PXGraphExtension && !Attributes.IsDefaultOrEmpty && 
			Attributes.Any(attrInfo => attrInfo.IsProtectedAccess);
	}
}