#nullable enable
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.GraphEvents;

/// <summary>
/// Information about the row event in graph.
/// </summary>
public class GraphRowEventInfo : GraphEventHandlerInfoBase<GraphRowEventInfo>
{
	
	public GraphRowEventInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
						  EventHandlerSignatureType signatureType, EventType eventType) :
					 base(node, symbol, declarationOrder, signatureType, eventType)
	{			
	}

	public GraphRowEventInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
						  EventHandlerSignatureType signatureType, EventType eventType, GraphRowEventInfo baseInfo)
				   : base(node, symbol, declarationOrder, signatureType, eventType, baseInfo)
	{		
	}

	internal override string GetEventGroupingKey() => $"{DacName}_{EventType.ToString()}";
}
