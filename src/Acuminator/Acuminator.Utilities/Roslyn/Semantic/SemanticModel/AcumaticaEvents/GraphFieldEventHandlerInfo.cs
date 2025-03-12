using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	/// <summary>
	/// Information about the graph event handlers related to the DAC fields.
	/// </summary>
	public class GraphFieldEventHandlerInfo : GraphFieldEventHandlerInfoBase<GraphFieldEventHandlerInfo>
	{
		public GraphFieldEventHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
										  EventHandlerLooseInfo handlerLooseInfo) :
									 base(node, symbol, declarationOrder, handlerLooseInfo)
		{
		}

		public GraphFieldEventHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
										  EventHandlerLooseInfo handlerLooseInfo, GraphFieldEventHandlerInfo baseInfo) :
									 base(node, symbol, declarationOrder, handlerLooseInfo, baseInfo)
		{
		}

		protected override void ValidateEventType(EventType eventType)
		{
			if (!eventType.IsDacFieldEvent())
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType} is not a field event type.");
			else if (eventType == EventType.CacheAttached)
			{
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType} is a cache attached event." +
													  $"The {nameof(GraphCacheAttachedEventHandlerInfo)} type should be used.");
			}
		}
	}
}
