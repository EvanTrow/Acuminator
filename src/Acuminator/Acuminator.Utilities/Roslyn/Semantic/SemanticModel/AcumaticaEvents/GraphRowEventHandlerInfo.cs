using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Information about the graph event handlers related to entire DAC records.
/// </summary>
public class GraphRowEventHandlerInfo : GraphEventHandlerInfoBase<GraphRowEventHandlerInfo>
{
	
	public GraphRowEventHandlerInfo(MethodDeclarationSyntax? handlerNode, IMethodSymbol handlerSymbol, int declarationOrder,
									EventHandlerLooseInfo handlerLooseInfo) :
							   base(handlerNode, handlerSymbol, declarationOrder, handlerLooseInfo)
	{			
	}

	public GraphRowEventHandlerInfo(MethodDeclarationSyntax? handlerNode, IMethodSymbol handlerSymbol, int declarationOrder,
									EventHandlerLooseInfo handlerLooseInfo, GraphRowEventHandlerInfo baseInfo) :
							   base(handlerNode, handlerSymbol, declarationOrder, handlerLooseInfo, baseInfo)
	{		
	}

	protected override void ValidateEventType(EventType eventType)
	{
		if (!eventType.IsDacRowEvent())
			throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType} is not a row event type.");
	}

	internal override string GetEventGroupingKey() => $"{DacName}_{EventType.ToString()}";
}
