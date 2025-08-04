using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Semantic information about the PXOverride method. 
	/// </summary>
	public class PXOverrideInfo : SymbolItem<IMethodSymbol>
	{
		public PXOverrideType OverrideType { get; }

		/// <summary>
		/// The base method overridden by the PXOverride method.
		/// </summary>
		public IMethodSymbol? BaseMethod { get; }

		public PXOverrideInfo(IMethodSymbol symbol, PXOverrideType pxOverrideType, IMethodSymbol? baseMethod, int declarationOrder) : 
						base(symbol, declarationOrder)
		{
			if (pxOverrideType == PXOverrideType.None)
				throw new ArgumentOutOfRangeException(nameof(pxOverrideType), pxOverrideType, $"PXOverride type must not be {PXOverrideType.None}");

			OverrideType = pxOverrideType;
			BaseMethod = baseMethod;
		}

		internal static IEnumerable<PXOverrideInfo> GetDeclaredPXOverrides(GraphOrGraphExtInfoBase graphExtensionInfo, PXContext context, 
																		   CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var pxOverrideAttribute = context.AttributeTypes.PXOverrideAttribute;

			if (pxOverrideAttribute == null)
				yield break;

			var directBaseTypesAndThis = graphExtensionInfo.Symbol.GetBaseTypesAndThis()
																  .ToList(capacity: 4);
			var graphAndGraphExtensionBaseTypes = graphExtensionInfo.JustOverridenItems()
																	.Select(info => info.Symbol)
																	.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
																	.Where(baseType => !directBaseTypesAndThis.Contains(baseType, SymbolEqualityComparer.Default))
																	.ToList(capacity: 4);

			var declaredMethods = graphExtensionInfo.Symbol.GetMethods();
			int declarationOrder = 0;

			foreach (IMethodSymbol method in declaredMethods)
			{
				cancellation.ThrowIfCancellationRequested();

				if (!method.HasPXOverrideAttribute(pxOverrideAttribute))
					continue;

				var baseMethod = GetSuitableBaseMethod(method, graphAndGraphExtensionBaseTypes);
				var pxOverrideType = GetPXOverrideType(method, baseMethod);

				if (pxOverrideType != PXOverrideType.None)
				{
					yield return new PXOverrideInfo(method, pxOverrideType, baseMethod, declarationOrder);
					declarationOrder++;
				}
			}
		}

		private static IMethodSymbol? GetSuitableBaseMethod(IMethodSymbol patchMethodWithPXOverride, List<INamedTypeSymbol> graphAndGraphExtensionBaseTypes)
		{
			foreach (var baseType in graphAndGraphExtensionBaseTypes)
			{
				var suitableBaseMethod = baseType.GetMethods(patchMethodWithPXOverride.Name)
												 .FirstOrDefault(patchMethodWithPXOverride.IsPXOverrideOf);
				if (suitableBaseMethod != null)
					return suitableBaseMethod;
			}

			return null;
		}

		private static PXOverrideType GetPXOverrideType(IMethodSymbol patchMethodWithPXOverride, IMethodSymbol? baseMethod)
		{
			var methodParameters = patchMethodWithPXOverride.Parameters;

			if (methodParameters.IsDefaultOrEmpty)
				return PXOverrideType.WithoutBaseDelegate;

			var lastParameter = methodParameters[^1];

			if (lastParameter.Type.TypeKind != TypeKind.Delegate)
				return PXOverrideType.WithoutBaseDelegate;

			if (baseMethod != null)
			{
				int baseMethodParametersCount = baseMethod.Parameters.Length;

				if (baseMethodParametersCount == methodParameters.Length)
					return PXOverrideType.WithoutBaseDelegate;
				else if (baseMethodParametersCount == (methodParameters.Length - 1))
					return PXOverrideType.WithValidBaseDelegate;
			}

			return patchMethodWithPXOverride.HasValidBaseDelegateParameter()
				? PXOverrideType.WithValidBaseDelegate
				: PXOverrideType.WithInvalidBaseDelegate;
		}
	}
}
