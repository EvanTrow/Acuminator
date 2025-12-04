using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RawGraphViewData = (Microsoft.CodeAnalysis.ISymbol ViewSymbol, Microsoft.CodeAnalysis.INamedTypeSymbol ViewType);

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo

{
	private const int EstimatedNumberOfViewDelegatesInGraph = 8;

	public abstract ITypeSymbol? GraphType { get; }

	protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol graphOrGraphExt, int declarationOrder) :
								 base(node, graphOrGraphExt, declarationOrder)
	{ }

	public abstract IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromDerivedExtensionToBaseGraph(bool includeSelf);

	public abstract IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromBaseGraphToDerivedExtension(bool includeSelf);


	



	
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

	private IEnumerable<TRawData> GetRawData<TRawData>(bool includeFromBaseInfos,
													   Func<GraphOrGraphExtInfoBase, IEnumerable<TRawData>> rawDataGetter)
	{
		if (includeFromBaseInfos)
		{
			return GetInfosFromBaseGraphToDerivedExtension(includeFromBaseInfos)
					  .SelectMany(graphOrGraphExtInfo => rawDataGetter(graphOrGraphExtInfo));
		}
		else
		{
			return rawDataGetter(this);
		}
	}

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
}