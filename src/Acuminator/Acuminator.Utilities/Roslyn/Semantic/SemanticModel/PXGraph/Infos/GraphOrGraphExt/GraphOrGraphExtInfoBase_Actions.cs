using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RawGraphActionData = (Microsoft.CodeAnalysis.ISymbol ActionSymbol, Microsoft.CodeAnalysis.ITypeSymbol ActionType);

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract partial class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo

{
	internal OverridableItemsCollection<ActionInfo> GetActionInfos(PXContext pxContext, CancellationToken cancellation)
	{
		const int estimatedNumberOfActionsInGraph = 8;

		var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(pxContext);
		var graphActionsByName = new OverridableItemsCollection<ActionInfo>(estimatedNumberOfActionsInGraph);
		var rawGraphActionDataFromBaseGraphToDerivedExtension = GetRawGraphActionsData(pxContext, includeFromBaseInfos: true, cancellation);

		int declarationOrder = 0;

		foreach (var (actionSymbol, actionType) in rawGraphActionDataFromBaseGraphToDerivedExtension)
		{
			cancellation.ThrowIfCancellationRequested();

			bool isSystemAction = systemActionsRegister.IsSystemAction(actionType);
			var graphActionInfo = new ActionInfo(actionSymbol, actionType, declarationOrder, isSystemAction);

			graphActionsByName.Add(graphActionInfo);
			declarationOrder++;
		}

		return graphActionsByName;
	}

	

	private IEnumerable<RawGraphActionData> GetRawGraphActionsData(PXContext pxContext, bool includeFromBaseInfos,
																   CancellationToken cancellation) =>
		GetRawData(includeFromBaseInfos, graphOrGraphExtInfo => GetRawGraphActionsData(graphOrGraphExtInfo, pxContext, cancellation));

	

	private static IEnumerable<RawGraphActionData> GetRawGraphActionsData(GraphOrGraphExtInfoBase graphOrGraphExtInfo, PXContext pxContext,
																		  CancellationToken cancellation)
	{
		var members = graphOrGraphExtInfo.Symbol.GetMembers();

		if (members.IsDefaultOrEmpty)
			yield break;

		foreach (IFieldSymbol field in members.OfType<IFieldSymbol>())
		{
			if (field.DeclaredAccessibility == Accessibility.Public && field.Type.IsPXAction())
			{
				yield return (field, field.Type);
			}
		}
	}

	
}