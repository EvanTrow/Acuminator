using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RawGraphViewData = (Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType);

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract partial class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo

{
	internal OverridableItemsCollection<DataViewDelegateInfo> GetViewDelegateInfos(PXContext pxContext, IDictionary<string, DataViewInfo> viewsByName,
																				   CancellationToken cancellation)
	{
		const int estimatedNumberOfViewDelegatesInGraph = 8;

		var graphViewDelegatesByName = new OverridableItemsCollection<DataViewDelegateInfo>(estimatedNumberOfViewDelegatesInGraph);
		var rawGraphViewDelegatesDataFromBaseGraphToDerivedExtension = 
			GetRawViewDelegatesData(pxContext, viewsByName, includeFromBaseInfos: true, cancellation);

		int declarationOrder = 0;

		foreach (var (viewDelegateNode, viewDelegateSymbol) in rawGraphViewDelegatesDataFromBaseGraphToDerivedExtension)
		{
			cancellation.ThrowIfCancellationRequested();
			var graphViewInfo = new DataViewDelegateInfo(viewDelegateNode, viewDelegateSymbol, declarationOrder);

			graphViewDelegatesByName.Add(graphViewInfo);
			declarationOrder++;
		}

		return graphViewDelegatesByName;
	}

	internal OverridableItemsCollection<DataViewInfo> GetViewInfos(PXContext pxContext, CancellationToken cancellation)
	{
		const int estimatedNumberOfViewsInGraph = 16;

		var graphViewsByName = new OverridableItemsCollection<DataViewInfo>(estimatedNumberOfViewsInGraph);
		var rawGraphViewDataFromBaseGraphToDerivedExtension = GetRawGraphViewsData(pxContext, includeFromBaseInfos: true, cancellation);

		int declarationOrder = 0;

		foreach (var (viewSymbol, viewType) in rawGraphViewDataFromBaseGraphToDerivedExtension)
		{
			cancellation.ThrowIfCancellationRequested();
			var graphViewInfo = new DataViewInfo(viewSymbol, viewType, pxContext, declarationOrder);

			graphViewsByName.Add(graphViewInfo);
			declarationOrder++;
		}

		return graphViewsByName;
	}

	private IEnumerable<RawGraphViewData> GetRawGraphViewsData(PXContext pxContext, bool includeFromBaseInfos, 
															   CancellationToken cancellation) =>
		GetRawData(includeFromBaseInfos, graphOrGraphExtInfo => GetRawGraphViewsData(graphOrGraphExtInfo, pxContext, cancellation));

	private IEnumerable<(MethodDeclarationSyntax? Node, IMethodSymbol Symbol)> GetRawViewDelegatesData(PXContext pxContext,
																							IDictionary<string, DataViewInfo> viewsByName,
																							bool includeFromBaseInfos, CancellationToken cancellation) =>
		GetRawData(includeFromBaseInfos, graphOrGraphExtInfo => GetRawViewDelegatesData(graphOrGraphExtInfo, viewsByName, pxContext, cancellation));

	private static IEnumerable<RawGraphViewData> GetRawGraphViewsData(GraphOrGraphExtInfoBase graphOrGraphExtInfo, PXContext pxContext,
																	  CancellationToken cancellation)
	{
		var members = graphOrGraphExtInfo.Symbol.GetMembers();

		if (members.IsDefaultOrEmpty)
			yield break;

		foreach (ISymbol member in members)
		{
			cancellation.ThrowIfCancellationRequested();

			var viewType = member switch
			{
				IFieldSymbol field 		 => field.Type as INamedTypeSymbol,
				IPropertySymbol property => property.Type as INamedTypeSymbol,
				_ 						 => null
			};

			if (viewType != null && member.DeclaredAccessibility == Accessibility.Public && viewType.InheritsFrom(pxContext.PXSelectBase.Type))
			{
				yield return (member, viewType);
			}	
		}
	}

	private static IEnumerable<(MethodDeclarationSyntax? Node, IMethodSymbol Symbol)> GetRawViewDelegatesData(GraphOrGraphExtInfoBase graphOrGraphExtInfo, 
																								IDictionary<string, DataViewInfo> viewsByName,
																								PXContext pxContext, CancellationToken cancellation)
	{
		var methods = graphOrGraphExtInfo.Symbol.GetMethods();

		foreach (IMethodSymbol method in methods)
		{
			cancellation.ThrowIfCancellationRequested();

			if (method.IsValidViewDelegate(pxContext) && viewsByName.ContainsKey(method.Name))
			{
				var node = method.GetSyntax(cancellation) as MethodDeclarationSyntax;
				yield return (node, method);
			}
		}
	}
}