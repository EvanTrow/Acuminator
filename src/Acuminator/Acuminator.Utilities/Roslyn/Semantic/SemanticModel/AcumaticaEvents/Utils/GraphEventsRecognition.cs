using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	public static class GraphEventsRecognition
	{
		private enum GraphEventHandlerKind : byte
		{
			NotRecognized,
			Regular,
			WithDelegateParameter
		}

		public static GraphEventHandlerInfoBase? TryRecognizeEventHandler(IMethodSymbol handlerSymbol, PXContext pxContext, int declarationOrder, 
																		  CancellationToken cancellation)
		{
			EventHandlerLooseInfo eventHandlerInfo = handlerSymbol.GetEventHandlerLooseInfo(pxContext);

			var graphEventHandlerKind = RecognizeGraphEventHandlerKind(handlerSymbol, eventHandlerInfo);
			if (graphEventHandlerKind == GraphEventHandlerKind.NotRecognized)
				return null;

			var handlerNode = handlerSymbol.GetSyntax(cancellation) as MethodDeclarationSyntax;

			switch (eventHandlerInfo.TargetKind)
			{
				case EventTargetKind.Row:
					return new GraphRowEventInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo);
				case EventTargetKind.Field:
					if (eventHandlerInfo.Type == EventType.CacheAttached)
						return new GraphCacheAttachedEventHandlerInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo);
					else
						return new GraphFieldEventInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo);
				default:
					return null;
			}
		}

		private static GraphEventHandlerKind RecognizeGraphEventHandlerKind(IMethodSymbol eventHandlerCandidate, EventHandlerLooseInfo handlerInfo)
		{
			if (handlerInfo.Type == EventType.None || handlerInfo.SignatureType == EventHandlerSignatureType.None ||
				handlerInfo.TargetKind == EventTargetKind.None)
			{
				return GraphEventHandlerKind.NotRecognized;
			}

			int parametersCount = eventHandlerCandidate.Parameters.Length;

			if (eventHandlerCandidate.CheckIfNull().IsStatic || !eventHandlerCandidate.ReturnsVoid || eventHandlerCandidate.IsGenericMethod ||
				parametersCount < 1 || parametersCount > 3)
			{
				return GraphEventHandlerKind.NotRecognized;
			}

			return handlerInfo.SignatureType switch
			{
				EventHandlerSignatureType.Classic => RecognizeClassicGraphEventHandlerKind(eventHandlerCandidate, handlerInfo, parametersCount),
				EventHandlerSignatureType.Generic => IsValidGenericGraphEventHandlerSignature(eventHandlerCandidate, parametersCount),
				_								  => GraphEventHandlerKind.NotRecognized
			};
		}

		private static GraphEventHandlerKind RecognizeClassicGraphEventHandlerKind(IMethodSymbol eventHandlerCandidate, EventHandlerLooseInfo handlerInfo,
																				   int parametersCount)
		{
			var graphEventHandlerKind = RecognizeClassicGraphEventHandlerKindFromParameretersCount(eventHandlerCandidate, handlerInfo, parametersCount);

			if (graphEventHandlerKind == GraphEventHandlerKind.NotRecognized)
				return GraphEventHandlerKind.NotRecognized;

			const char underscore = '_';

			if (eventHandlerCandidate.Name[0] == underscore || eventHandlerCandidate.Name[^1] == underscore)
				return GraphEventHandlerKind.NotRecognized;

			int underscoresCount = eventHandlerCandidate.Name.Count(c => c == underscore);

			if ((handlerInfo.TargetKind == EventTargetKind.Row && underscoresCount != 1) ||
				(handlerInfo.TargetKind == EventTargetKind.Field && underscoresCount != 2))
			{
				return GraphEventHandlerKind.NotRecognized;
			}

			if (graphEventHandlerKind == GraphEventHandlerKind.Regular)
				return GraphEventHandlerKind.Regular;
			else
			{
				return eventHandlerCandidate.HasValidBaseDelegateParameter()
					? GraphEventHandlerKind.WithDelegateParameter
					: GraphEventHandlerKind.NotRecognized;
			}
		}

		private static GraphEventHandlerKind RecognizeClassicGraphEventHandlerKindFromParameretersCount(IMethodSymbol eventHandlerCandidate,
																										EventHandlerLooseInfo handlerInfo,
																										int parametersCount)
		{
			int regularHandlerParametersCount = handlerInfo.Type == EventType.CacheAttached ? 1 : 2;

			if (parametersCount == regularHandlerParametersCount)
				return GraphEventHandlerKind.Regular;
			else if (parametersCount == (regularHandlerParametersCount + 1))
				return GraphEventHandlerKind.WithDelegateParameter;
			else
				return GraphEventHandlerKind.NotRecognized;
		}

		private static GraphEventHandlerKind IsValidGenericGraphEventHandlerSignature(IMethodSymbol eventHandlerCandidate, int parametersCount)
		{
			switch (parametersCount)
			{
				case 1:
					return GraphEventHandlerKind.Regular;
				case 2:
					return eventHandlerCandidate.HasValidBaseDelegateParameter()
						? GraphEventHandlerKind.WithDelegateParameter
						: GraphEventHandlerKind.NotRecognized;
				default:
					return GraphEventHandlerKind.NotRecognized;
			}
		}
	}
}
