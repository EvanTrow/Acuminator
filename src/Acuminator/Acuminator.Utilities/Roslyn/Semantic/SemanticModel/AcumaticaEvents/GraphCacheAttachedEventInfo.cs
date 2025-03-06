using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	/// <summary>
	/// Information about the cache attached graph event handler.
	/// </summary>
	public class GraphCacheAttachedEventInfo : GraphFieldEventInfoBase<GraphCacheAttachedEventInfo>
	{
		public GraphCacheAttachedEventInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
											EventHandlerLooseInfo handlerLooseInfo) :
									  base(node, symbol, declarationOrder, handlerLooseInfo)
		{
		}

		public GraphCacheAttachedEventInfo(MethodDeclarationSyntax? node, IMethodSymbol symbol, int declarationOrder,
											EventHandlerLooseInfo handlerLooseInfo, GraphCacheAttachedEventInfo baseInfo) :
									  base(node, symbol, declarationOrder, handlerLooseInfo, baseInfo)
		{
		}

		protected override void ValidateEventType(EventType eventType)
		{
			if (eventType != EventType.CacheAttached)
				throw new ArgumentOutOfRangeException(nameof(eventType), $"The {eventType} is not a cache attached event.");
		}

		protected override void CombineWithBaseInfo(GraphCacheAttachedEventInfo baseInfo)
		{
			// TODO implement merge of attributes declared on cache attached event handlers
		}
	}
}
