using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Helper that checks if method is Acumatica override.
	/// </summary>
	public static class AcumaticaOverridesHelper
	{
		private enum MethodsCompatibility
		{
			NotCompatible,
			ParametersMatch,
			ParametersMatchWithDelegate
		}

		/// <summary>
		/// Special signature check between a derived method with the PXOverride attribute and the base method.
		/// </summary>
		/// <param name="pxOverrideMethod">The method from the derived class, with the PXOverride attribute</param>
		/// <param name="baseMethod">The method from the base</param>
		/// <returns></returns>
		public static bool IsPXOverrideOf(this IMethodSymbol pxOverrideMethod, IMethodSymbol baseMethod)
		{
			pxOverrideMethod.ThrowOnNull();

			var methodsCompatibility = GetMethodsCompatibility(baseMethod.Parameters.Length, pxOverrideMethod.Parameters.Length);

			if (methodsCompatibility == MethodsCompatibility.NotCompatible ||
				!baseMethod.CanBeOverriden() || !baseMethod.IsAccessibleOutsideOfAssembly())
			{
				return false;
			}

			if (methodsCompatibility == MethodsCompatibility.ParametersMatch)
				return pxOverrideMethod.SignatureEquals(baseMethod);

			if (methodsCompatibility == MethodsCompatibility.ParametersMatchWithDelegate)
			{
				if (pxOverrideMethod.Parameters[pxOverrideMethod.Parameters.Length - 1].Type is not INamedTypeSymbol @delegate ||
					@delegate.TypeKind != TypeKind.Delegate)
				{
					return false;
				}

				return baseMethod.Parameters.EqualsParameterRange(pxOverrideMethod.Parameters, rangeStart: 0, rangeEnd: baseMethod.Parameters.Length) &&
					   baseMethod.SignatureEquals(@delegate.DelegateInvokeMethod);
			}

			return false;
		}

		private static MethodsCompatibility GetMethodsCompatibility(int baseMethodParametersCount, int pxOverrideMethodParametersCount)
		{
			return baseMethodParametersCount == pxOverrideMethodParametersCount
				? MethodsCompatibility.ParametersMatch
				: (baseMethodParametersCount + 1) == pxOverrideMethodParametersCount
					? MethodsCompatibility.ParametersMatchWithDelegate
					: MethodsCompatibility.NotCompatible;
		}
	}
}
