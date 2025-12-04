#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphViewSymbolUtils
	{
		/// <summary>
		/// Returns true if the data view is a processing view
		/// </summary>
		/// <param name="view">The type symbol of a data view</param>
		/// <param name="pxContext">The context</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsProcessingView(this ITypeSymbol view, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			return view.CheckIfNull().InheritsFromOrEqualsGeneric(pxContext.PXProcessingBase.Type);
		}

		/// <summary>
		/// Get the view delegates symbols and syntax nodes from the graph. The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graph">The graph to act on.</param>
		/// <param name="viewsByName">The views of the graph dictionary with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.</param>
		/// <param name="pxContext">Context.</param>
		/// <param name="inheritance">(Optional) If true includes view delegates from the graph inheritance chain.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// The view delegates from graph.
		/// </returns>
		public static OverridableItemsCollection<DataViewDelegateInfo> GetViewDelegatesFromGraph(this ITypeSymbol graph,
																						   IDictionary<string, DataViewInfo> viewsByName,
																						   PXContext pxContext, bool inheritance = true, 
																						   CancellationToken cancellation = default)
		{
			viewsByName.ThrowOnNull();

			if (!graph.IsPXGraph(pxContext))
				return new OverridableItemsCollection<DataViewDelegateInfo>();

			var viewDelegatesByName = new OverridableItemsCollection<DataViewDelegateInfo>(capacity: EstimatedNumberOfViewDelegatesInGraph);
			var graphViewDelegates = GetRawViewDelegatesFromGraphImpl(graph, viewsByName, pxContext, inheritance, cancellation);

			viewDelegatesByName.AddRangeWithDeclarationOrder(graphViewDelegates, startingOrder: 0, 
															 (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			return viewDelegatesByName;
		}

		/// <summary>
		/// Get the view delegates symbols and syntax nodes from the graph extension.
		/// The <paramref name="viewsByName"/> must have <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on</param>
		/// <param name="viewsByName">The views of the graph extension with <see cref="StringComparer.OrdinalIgnoreCase"/> comparer</param>
		/// <param name="pxContext">Context</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns/>
		public static OverridableItemsCollection<DataViewDelegateInfo> GetViewDelegatesFromGraphExtensionAndBaseGraph(this ITypeSymbol graphExtension,
																							IDictionary<string, DataViewInfo> viewsByName,
																							PXContext pxContext, CancellationToken cancellation)
		{
			graphExtension.ThrowOnNull();
			viewsByName.ThrowOnNull();
			pxContext.ThrowOnNull();

			return GetViewInfoFromGraphExtension<DataViewDelegateInfo>(graphExtension, pxContext, AddDelegatesFromGraph, AddDelegatesFromGraphExtension);

			int AddDelegatesFromGraph(OverridableItemsCollection<DataViewDelegateInfo> delegates, ITypeSymbol graph, int startingOrder)
			{
				var graphViewDelegates = graph.GetRawViewDelegatesFromGraphImpl(viewsByName, pxContext, inheritance: true, cancellation);
				return delegates.AddRangeWithDeclarationOrder(graphViewDelegates, startingOrder, 
															  (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			}

			int AddDelegatesFromGraphExtension(OverridableItemsCollection<DataViewDelegateInfo> delegates, ITypeSymbol graphExt, int startingOrder)
			{
				var extensionViewDelegates = graphExt.GetRawViewDelegatesFromGraphOrGraphExtension(viewsByName, pxContext, cancellation);
				return delegates.AddRangeWithDeclarationOrder(extensionViewDelegates, startingOrder, 
															  (viewDel, order) => new DataViewDelegateInfo(viewDel.Node, viewDel.Symbol, order));
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax? Node, IMethodSymbol Symbol)> GetRawViewDelegatesFromGraphImpl(
																	this ITypeSymbol graph, IDictionary<string, DataViewInfo> viewsByName,
																	PXContext pxContext, bool inheritance, CancellationToken cancellation)
		{
			if (inheritance)
			{
				return graph.GetBaseTypesAndThis()
							.TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
							.Reverse()
							.SelectMany(baseGraph => GetRawViewDelegatesFromGraphOrGraphExtension(baseGraph, viewsByName, pxContext, cancellation));
			}
			else
			{
				return GetRawViewDelegatesFromGraphOrGraphExtension(graph, viewsByName, pxContext, cancellation);
			}
		}

		private static IEnumerable<(MethodDeclarationSyntax? Node, IMethodSymbol Symbol)> GetRawViewDelegatesFromGraphOrGraphExtension(
															this ITypeSymbol graphOrExtension, IDictionary<string, DataViewInfo> viewsByName,
															PXContext pxContext, CancellationToken cancellation)
		{
			IEnumerable<IMethodSymbol> delegates = from method in graphOrExtension.GetMethods()
												   where method.IsValidViewDelegate(pxContext) && viewsByName.ContainsKey(method.Name)
												   select method;

			foreach (IMethodSymbol viewDelegage in delegates)
			{
				cancellation.ThrowIfCancellationRequested();

				var declaration = viewDelegage.GetSyntax(cancellation) as MethodDeclarationSyntax;
				yield return (declaration, viewDelegage);
			}
		}

		
	}
}