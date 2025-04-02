using System;
using System.Collections.Generic;
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

			var graphEventHandlerKind = RecognizeGraphEventHandlerKind(handlerSymbol, eventHandlerInfo, pxContext);
			if (graphEventHandlerKind == GraphEventHandlerKind.NotRecognized)
				return null;

			var handlerNode = handlerSymbol.GetSyntax(cancellation) as MethodDeclarationSyntax;

			switch (eventHandlerInfo.TargetKind)
			{
				case EventTargetKind.Row:
					return new GraphRowEventHandlerInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo, pxContext);
				case EventTargetKind.Field:
					if (eventHandlerInfo.Type == EventType.CacheAttached)
						return new GraphCacheAttachedEventHandlerInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo, pxContext);
					else
						return new GraphFieldEventHandlerInfo(handlerNode, handlerSymbol, declarationOrder, eventHandlerInfo, pxContext);
				default:
					return null;
			}
		}

		private static GraphEventHandlerKind RecognizeGraphEventHandlerKind(IMethodSymbol eventHandlerCandidate, EventHandlerLooseInfo handlerInfo, 
																			PXContext pxContext)
		{
			if (handlerInfo.Type == EventType.None || handlerInfo.SignatureType == EventHandlerSignatureType.None ||
				handlerInfo.TargetKind == EventTargetKind.None)
			{
				return GraphEventHandlerKind.NotRecognized;
			}

			int parametersCount = eventHandlerCandidate.Parameters.Length;

			if (!eventHandlerCandidate.CheckIfNull().ReturnsVoid || eventHandlerCandidate.IsGenericMethod || parametersCount < 1 || parametersCount > 3)
				return GraphEventHandlerKind.NotRecognized;

			return handlerInfo.SignatureType switch
			{
				EventHandlerSignatureType.Classic => RecognizeClassicGraphEventHandlerKind(eventHandlerCandidate, handlerInfo, pxContext, parametersCount),
				EventHandlerSignatureType.Generic => IsValidGenericGraphEventHandlerSignature(eventHandlerCandidate, handlerInfo, pxContext, parametersCount),
				_								  => GraphEventHandlerKind.NotRecognized
			};
		}

		private static GraphEventHandlerKind RecognizeClassicGraphEventHandlerKind(IMethodSymbol eventHandlerCandidate, EventHandlerLooseInfo handlerInfo,
																				   PXContext pxContext, int parametersCount)
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
				if (handlerInfo.Type == EventType.CacheAttached)
					return RecognizeCacheAttachedEventHandlerWithExtraParameter(eventHandlerCandidate, pxContext);
				else
				{
					return eventHandlerCandidate.HasValidBaseDelegateParameter()
						? GraphEventHandlerKind.WithDelegateParameter
						: GraphEventHandlerKind.NotRecognized;
				}
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

		private static GraphEventHandlerKind RecognizeCacheAttachedEventHandlerWithExtraParameter(IMethodSymbol eventHandlerCandidate, PXContext pxContext)
		{
			// Cache attached event handlers do not support overrides with interceptors.
			// However, PXOverrides should be supported
			if (eventHandlerCandidate.HasPXOverrideAttribute(pxContext))
			{
				return eventHandlerCandidate.HasValidBaseDelegateParameter()
						? GraphEventHandlerKind.WithDelegateParameter
						: GraphEventHandlerKind.NotRecognized;
			}
			else
				return GraphEventHandlerKind.NotRecognized;
		}

		private static GraphEventHandlerKind IsValidGenericGraphEventHandlerSignature(IMethodSymbol eventHandlerCandidate, EventHandlerLooseInfo handlerInfo,
																					  PXContext pxContext, int parametersCount)
		{
			switch (parametersCount)
			{
				case 1:
					return GraphEventHandlerKind.Regular;
				case 2:
					return HasValidDelegateParameterForGenericEventHandlerOverride(eventHandlerCandidate, handlerInfo, pxContext)
						? GraphEventHandlerKind.WithDelegateParameter
						: GraphEventHandlerKind.NotRecognized;
				default:
					return GraphEventHandlerKind.NotRecognized;
			}
		}

		private static bool HasValidDelegateParameterForGenericEventHandlerOverride(IMethodSymbol genericEventHandlerCandidate, 
																					EventHandlerLooseInfo handlerInfo, PXContext pxContext)
		{
			var delegateParameter = genericEventHandlerCandidate.Parameters[^1];

			if (delegateParameter.Type is not INamedTypeSymbol delegateType || delegateType.TypeKind != TypeKind.Delegate ||
				delegateType.DelegateInvokeMethod is not { } baseDelegateMethod)
			{
				return false;
			}

			if (!baseDelegateMethod.ReturnsVoid || baseDelegateMethod.IsGenericMethod)
				return false;

			// First we check if the generic event handler is a PXOverride attribute.
			// Generic event handlers do not support PXOverrides due to a bug https://jira.acumatica.com/browse/AC-302387 in Acumatica Framework.
			// However, we still should recognize such PXOverrides as generic event handlers + the bug will be fixed in the future.
			if (genericEventHandlerCandidate.HasPXOverrideAttribute(pxContext))
			{
				// For PXOverrides the delegate parameter's signature should be the same as the base method's signature
				return genericEventHandlerCandidate.HasValidBaseDelegateParameter();
			}

			// Cache attached event handlers do not support overrides with interceptors and we already checked that this is not PXOverride.
			if (handlerInfo.Type == EventType.CacheAttached)
				return false;

			// Check for if the event handler is an override with the interceptor mechanism. 
			// The delegate type of the delegate parameter should have classic event handler signature.
			var parameters = baseDelegateMethod.Parameters;

			if (parameters.Length != 2 || !parameters[0].Type.Equals(pxContext.PXCache.Type, SymbolEqualityComparer.Default) ||
				!pxContext.Events.EventTypeToClassicEventArgTypeMap.TryGetValue(handlerInfo.Type, out var classicEventArgType))
			{
				return false;
			}

			return parameters[1].Type.Equals(classicEventArgType, SymbolEqualityComparer.Default);
		}
	}
}
