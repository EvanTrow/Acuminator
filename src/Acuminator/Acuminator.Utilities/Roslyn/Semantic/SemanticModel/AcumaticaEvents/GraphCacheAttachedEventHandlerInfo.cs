using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	/// <summary>
	/// Information about the cache attached graph event handler.
	/// </summary>
	public class GraphCacheAttachedEventHandlerInfo : GraphFieldEventHandlerInfoBase<GraphCacheAttachedEventHandlerInfo>
	{
		public GraphCacheAttachedEventHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
												  EventHandlerLooseInfo handlerLooseInfo, PXContext pxContext) :
											 base(node, symbol, declarationOrder, handlerLooseInfo, pxContext)
		{
		}

		public GraphCacheAttachedEventHandlerInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
												  EventHandlerLooseInfo handlerLooseInfo, GraphCacheAttachedEventHandlerInfo baseInfo, 
												  PXContext pxContext) :
											 base(node, symbol, declarationOrder, handlerLooseInfo, baseInfo, pxContext)
		{
		}

		protected override void ValidateEventType(EventType eventType)
		{
			if (eventType != EventType.CacheAttached)
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType} is not a cache attached event.");
		}

		protected override void CombineWithBaseInfo()
		{
			// TODO implement merge of attributes declared on cache attached event handlers
		}
	}
}
