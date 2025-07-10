#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph
{
	public class GraphSemanticModelTests : SemanticModelTestsBase<PXGraphSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("2ndLevelGraphExtension.cs")]
		public async Task SecondLevel_Derived_GraphExtension_InfoCollection(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);
			var graphExtensionInfo = graphSemanticModel.GraphOrGraphExtInfo as GraphExtensionInfo;

			graphExtensionInfo.Should().NotBeNull();
			graphExtensionInfo!.Graph.Should().NotBeNull();
			graphExtensionInfo!.Graph!.Name.Should().Be("MyGraph");
			graphExtensionInfo.Base.Should().NotBeNull();
			graphExtensionInfo.Base!.Name.Should().Be("SecondLevelGraphExtension");

			var extensionFromPreviousLevel = graphExtensionInfo.Base!.Base as GraphExtensionInfo;

			extensionFromPreviousLevel.Should().NotBeNull();
			extensionFromPreviousLevel!.Name.Should().Be("BaseExtension");
			extensionFromPreviousLevel!.Graph.Should().NotBeNull();
			extensionFromPreviousLevel.Base.Should().NotBeNull();
			extensionFromPreviousLevel.Base.Should().Be(extensionFromPreviousLevel.Graph);
		}

		[Theory]
		[EmbeddedFileData("GraphWithSetupViews.cs")]
		public async Task Graph_WithSetupViews_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			foreach (var view in graphSemanticModel.Views)
			{
				view.IsSetup.Should().BeTrue();
			}
		}

		#region Initialize Method Tests
		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithExplicitInitializeMethod.cs")]
		public async Task Graph_WithExplicitInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithImplicitInitializeMethod.cs")]
		public async Task Graph_WithImplicitInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}
		
		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithIncorrectInitializeMethod.cs")]
		public async Task Graph_WithIncorrectInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().BeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphExtensionWithInitializeMethod.cs")]
		public async Task GraphExtension_WithInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraphExtension);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphExtensionWithIncorrectInitializeMethod.cs")]
		public async Task GraphExtension_WithIncorrectInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			var graphExtInfo = graphSemanticModel.GraphOrGraphExtInfo as GraphExtensionInfo;

			graphExtInfo.Should().NotBeNull();
			graphExtInfo!.Graph.Should().NotBeNull();
			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraphExtension);
			graphSemanticModel.InitializeMethodInfo.Should().BeNull();
		}
		#endregion

		#region Graph Extension Overrides
		[Theory]
		[EmbeddedFileData(@"EventOverrides\GraphExtensionsWithOverrides.cs")]
		public async Task GraphExtensions_Overrides_Recognition(string text)
		{
			var graphExtensionModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);
			var graphExtensionModelWithEvents = PXGraphEventSemanticModel.EnrichGraphModelWithEvents(graphExtensionModel, CancellationToken.None);
			var declaredEventHandlers = graphExtensionModelWithEvents.DeclaredEventHandlers;
			var allEventHandlersOverridesChains = graphExtensionModelWithEvents.AllEventHandlerOverridesChains;

			graphExtensionModelWithEvents.Should().NotBeNull();
			graphExtensionModelWithEvents.PXOverrides.Should().HaveCount(3);

			#region Graph Hierarchy 
			graphExtensionModelWithEvents.GraphOrGraphExtInfo.Base.Should().NotBeNull();
			var baseExtension = graphExtensionModelWithEvents.GraphOrGraphExtInfo.Base!;
			baseExtension.Should().NotBeNull();
			baseExtension.Name.Should().Be("SomeGraphExtension");

			baseExtension.Base.Should().NotBeNull();
			var graph = baseExtension.Base!.Symbol;
			graph.Should().NotBeNull();
			graph.Should().Be(graphExtensionModel.GraphSymbol);
			graph!.Name.Should().Be("SomeGraph");
			#endregion

			#region Storages Check
			declaredEventHandlers.AllEventHandlersCount.Should().BePositive();
			allEventHandlersOverridesChains.AllEventHandlersCount.Should().BeGreaterThan(declaredEventHandlers.AllEventHandlersCount);
			#endregion

			CheckCacheAttachedEventHandlers();
			CheckFieldUpdatingEventHandlers();
			CheckFieldUpdatedEventHandlers();
			CheckFieldSelectingEventHandlers();
			CheckRowUpdatingEventHandlers();
			CheckRowUpdatedEventHandlers();

			//------------------------------------------------Local Function------------------------------------------------
			void CheckCacheAttachedEventHandlers()
			{
				declaredEventHandlers.CacheAttachedByName.Should().HaveCount(1);
				allEventHandlersOverridesChains.CacheAttachedByName.Should().HaveCount(2);

				var cacheAttachedDeclared = declaredEventHandlers.CacheAttachedEventHandlers.First();
				var cacheAttachedOverrides = allEventHandlersOverridesChains.CacheAttachedEventHandlers.First(h => h.Name == cacheAttachedDeclared.Name);

				cacheAttachedDeclared.Should().Be(cacheAttachedOverrides);

				CheckEventHandler(cacheAttachedDeclared, containingType: graphExtensionModelWithEvents.Symbol, dacName: "SomeDac", fieldName: "BranchID",
								  GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute, hasBaseDelegate: true,
								  EventType.CacheAttached, EventHandlerSignatureType.Classic, EventTargetKind.Field);

				var cacheAttachedInGraph = cacheAttachedOverrides.Base!;
				CheckEventHandler(cacheAttachedInGraph, containingType: graph, dacName: "SomeDac", fieldName: "BranchID",
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false,
								  EventType.CacheAttached, EventHandlerSignatureType.Classic, EventTargetKind.Field);
			}
			//-----------------------------------------------------------------------------
			void CheckFieldUpdatingEventHandlers()
			{
				declaredEventHandlers.FieldUpdatingByName.Should().HaveCount(1);
				allEventHandlersOverridesChains.FieldUpdatingByName.Should().HaveCount(1);

				var fieldUpdatingDeclared = declaredEventHandlers.FieldUpdatingEventHandlers.First();
				var fieldUpdatingOverrides = allEventHandlersOverridesChains.FieldUpdatingEventHandlers.First();
				fieldUpdatingDeclared.Should().Be(fieldUpdatingOverrides);

				CheckEventHandler(fieldUpdatingDeclared, containingType: graphExtensionModelWithEvents.Symbol, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: true,
								  EventType.FieldUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var fieldUpdatingBaseExtension = fieldUpdatingOverrides.Base!;
				CheckEventHandler(fieldUpdatingBaseExtension, containingType: baseExtension.Symbol, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: true,
								  EventType.FieldUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var fieldUpdatingInGraph = fieldUpdatingBaseExtension.Base!;
				CheckEventHandler(fieldUpdatingInGraph, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false,
								  EventType.FieldUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Field);
			}
			//-----------------------------------------------------------------------------
			void CheckFieldUpdatedEventHandlers()
			{
				declaredEventHandlers.FieldUpdatedByName.Should().HaveCount(1);
				allEventHandlersOverridesChains.FieldUpdatedByName.Should().HaveCount(1);

				var fieldUpdatedDeclared = declaredEventHandlers.FieldUpdatedEventHandlers.First();
				var fieldUpdatedOverrides = allEventHandlersOverridesChains.FieldUpdatedEventHandlers.First();
				fieldUpdatedDeclared.Should().Be(fieldUpdatedOverrides);

				CheckEventHandler(fieldUpdatedDeclared, containingType: graphExtensionModelWithEvents.Symbol, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: true,
								  EventType.FieldUpdated, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var fieldUpdatedBaseExtension = fieldUpdatedOverrides.Base!;
				CheckEventHandler(fieldUpdatedBaseExtension, containingType: baseExtension.Symbol, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: true,
								  EventType.FieldUpdated, EventHandlerSignatureType.Classic, EventTargetKind.Field);

				var allFieldUpdatedInBaseExtension = fieldUpdatedOverrides.JustOverridenItems()
																		  .Where(h => h.Symbol.IsDeclaredInType(baseExtension.Symbol))
																		  .ToList();
				allFieldUpdatedInBaseExtension.Should().HaveCount(1);
				allFieldUpdatedInBaseExtension.Should().Contain(fieldUpdatedBaseExtension);

				var fieldUpdatedInGraph = fieldUpdatedBaseExtension.Base!;
				CheckEventHandler(fieldUpdatedInGraph, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: false,
								  EventType.FieldUpdated, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				CheckEventHandler(fieldUpdatedInGraph.Base!, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false,
								  EventType.FieldUpdated, EventHandlerSignatureType.Classic, EventTargetKind.Field);

				var allFieldUpdatedInGraph = fieldUpdatedOverrides.JustOverridenItems()
																  .Where(h => h.Symbol.IsDeclaredInType(graph))
																  .ToList();
				allFieldUpdatedInGraph.Should().HaveCount(2);
			}
			//-----------------------------------------------------------------------------
			void CheckFieldSelectingEventHandlers()
			{
				declaredEventHandlers.FieldSelectingByName.Should().BeEmpty();
				allEventHandlersOverridesChains.FieldSelectingByName.Should().HaveCount(1);

				var fieldSelectingOverrides = allEventHandlersOverridesChains.FieldSelectingEventHandlers.First();
				CheckEventHandler(fieldSelectingOverrides, baseExtension.Symbol, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: false,
								  EventType.FieldSelecting, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var fieldSelectingInGraphWithDelegateParameter = fieldSelectingOverrides.Base!;
				CheckEventHandler(fieldSelectingInGraphWithDelegateParameter, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: true,
								  EventType.FieldSelecting, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var secondFieldSelectingInGraphWithoutDelegateParameter = fieldSelectingInGraphWithDelegateParameter.Base!;
				CheckEventHandler(secondFieldSelectingInGraphWithoutDelegateParameter, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.OverrideWithInterceptor, hasBaseDelegate: false,
								  EventType.FieldSelecting, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var firstFieldSelectingInGraphWithoutDelegateParameter = secondFieldSelectingInGraphWithoutDelegateParameter.Base!;
				CheckEventHandler(firstFieldSelectingInGraphWithoutDelegateParameter, containingType: graph, dacName: "SomeDac", fieldName: "DocBal",
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false,
								  EventType.FieldSelecting, EventHandlerSignatureType.Generic, EventTargetKind.Field);

				var fieldSelectingEventHandlersInGraph = fieldSelectingOverrides.JustOverridenItems()
																				.Where(h => h.Symbol.IsDeclaredInType(graph))
																				.ToList();
				fieldSelectingEventHandlersInGraph.Should().HaveCount(3);
				fieldSelectingEventHandlersInGraph.Should().Contain(fieldSelectingInGraphWithDelegateParameter);
				fieldSelectingEventHandlersInGraph.Should().Contain(secondFieldSelectingInGraphWithoutDelegateParameter);
				fieldSelectingEventHandlersInGraph.Should().Contain(firstFieldSelectingInGraphWithoutDelegateParameter);
			}
			//-----------------------------------------------------------------------------
			void CheckRowUpdatingEventHandlers()
			{
				declaredEventHandlers.RowUpdatingByName.Should().HaveCount(1);
				allEventHandlersOverridesChains.RowUpdatingByName.Should().HaveCount(1);

				var rowUpdatingDeclared = declaredEventHandlers.RowUpdatingEventHandlers.First();
				var rowUpdatingOverrides = allEventHandlersOverridesChains.RowUpdatingEventHandlers.First();
				rowUpdatingDeclared.Should().Be(rowUpdatingOverrides);

				CheckEventHandler(rowUpdatingDeclared, containingType: graphExtensionModelWithEvents.Symbol, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute, hasBaseDelegate: true,
								  EventType.RowUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Row);

				var rowUpdatingBaseExtension = rowUpdatingOverrides.Base!;
				CheckEventHandler(rowUpdatingBaseExtension, containingType: baseExtension.Symbol, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute, hasBaseDelegate: false,
								  EventType.RowUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Row);

				var rowUpdatingInGraph = rowUpdatingBaseExtension.Base!;
				CheckEventHandler(rowUpdatingInGraph, containingType: graph, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false,
								  EventType.RowUpdating, EventHandlerSignatureType.Generic, EventTargetKind.Row);
			}
			//-----------------------------------------------------------------------------
			void CheckRowUpdatedEventHandlers()
			{
				declaredEventHandlers.RowUpdatedByName.Should().BeEmpty();
				allEventHandlersOverridesChains.RowUpdatedByName.Should().HaveCount(1);

				var rowUpdatedWithDelegateParameterInBaseExtension = allEventHandlersOverridesChains.RowUpdatedEventHandlers.First();
				CheckEventHandler(rowUpdatedWithDelegateParameterInBaseExtension, containingType: baseExtension.Symbol, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute, hasBaseDelegate: true,
								  EventType.RowUpdated, EventHandlerSignatureType.Classic, EventTargetKind.Row);

				var rowUpdatedWithoutDelegateParameterInBaseExtension = rowUpdatedWithDelegateParameterInBaseExtension.Base!;
				CheckEventHandler(rowUpdatedWithoutDelegateParameterInBaseExtension, containingType: baseExtension.Symbol, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute, hasBaseDelegate: false,
								  EventType.RowUpdated, EventHandlerSignatureType.Classic, EventTargetKind.Row);

				var rowUpdatedInGraph = rowUpdatedWithoutDelegateParameterInBaseExtension.Base!;
				CheckEventHandler(rowUpdatedInGraph, containingType: graph, dacName: "SomeDac", fieldName: null,
								  GraphEventHandlerOverrideType.None, hasBaseDelegate: false, EventType.RowUpdated, 
								  EventHandlerSignatureType.Classic, EventTargetKind.Row);
			}
		}

		private void CheckEventHandler<THandler>(THandler eventHandler, ITypeSymbol containingType, string dacName, string? fieldName,
												 GraphEventHandlerOverrideType overrideType, bool hasBaseDelegate,
												 EventType eventType, EventHandlerSignatureType signatureType, EventTargetKind targetKind)
		where THandler : GraphEventHandlerInfoBase<THandler>
		{
			eventHandler.Symbol.ContainingType.Should().Be(containingType);
			eventHandler.DacName.Should().Be(dacName);

			if (!fieldName.IsNullOrWhiteSpace())
			{
				if (eventHandler is GraphFieldEventHandlerInfo fieldEventHandler)
					fieldEventHandler.DacFieldName.Should().Be(fieldName);
				else if (eventHandler is GraphCacheAttachedEventHandlerInfo cacheAttachedEventHandler)
					cacheAttachedEventHandler.DacFieldName.Should().Be(fieldName);
			}

			eventHandler.TargetKind.Should().Be(targetKind);
			eventHandler.EventType.Should().Be(eventType);
			eventHandler.SignatureType.Should().Be(signatureType);

			eventHandler.OverrideType.Should().Be(overrideType);
			(eventHandler.BaseDelegate != null).Should().Be(hasBaseDelegate);

			var (expectedCSharpOverride, expectedPXOverride) = overrideType switch
			{
				GraphEventHandlerOverrideType.CSharp 						   => (true, false),
				GraphEventHandlerOverrideType.OverrideWithInterceptor 		   => (false, false),
				GraphEventHandlerOverrideType.OverrrideWithPXOverrideAttribute => (false, true),
				_ 															   => (false, false)
			};

			eventHandler.IsCSharpOverride.Should().Be(expectedCSharpOverride);
			eventHandler.IsPXOverride.Should().Be(expectedPXOverride);

			if (overrideType != GraphEventHandlerOverrideType.None)
				eventHandler.Base.Should().NotBeNull();
		}

		#endregion

		protected override Task<PXGraphSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration);
			graphOrGraphExtSymbol.Should().NotBeNull();

			var graphSemanticModel = PXGraphSemanticModel.InferModel(context.PXContext, graphOrGraphExtSymbol!,
																	 GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																	 cancellation: cancellation);
			graphSemanticModel.Should().NotBeNull();
			graphSemanticModel!.GraphOrGraphExtInfo.Should().NotBeNull();

			return Task.FromResult(graphSemanticModel);
		}
	}
}
