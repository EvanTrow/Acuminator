using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature
{
	public class InvalidPXActionSignatureAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature);

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var actionHandlerWithBadSignature = from method in pxGraph.Symbol.GetMethods()
												where method.IsDeclaredInType(pxGraph.Symbol) &&
													  pxGraph.ActionsByNames.ContainsKey(method.Name) &&
													  IsActionDelegateWithBadSignature(method, pxContext, pxGraph)
												select method;

			foreach (IMethodSymbol method in actionHandlerWithBadSignature)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();
				Location? methodLocation = method.Locations.FirstOrDefault();

				if (methodLocation != null)
				{
					symbolContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature, methodLocation),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static bool IsActionDelegateWithBadSignature(IMethodSymbol method, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
		{
			var parameters = method.Parameters;
			var pxOverrideInfoForActionDelegate = 
				pxGraph.DeclaredPXOverrides
					   .FirstOrDefault(pxOverrideInfo => pxOverrideInfo.Symbol.Equals(method, SymbolEqualityComparer.Default) ||
														 pxOverrideInfo.Symbol.OriginalDefinition.Equals(method.OriginalDefinition, 
																										 SymbolEqualityComparer.Default));
			bool hasBaseDelegateParameter = pxOverrideInfoForActionDelegate != null && 
											pxOverrideInfoForActionDelegate.OverrideType != PXOverrideType.WithoutBaseDelegate;
			if (method.ReturnsVoid)
			{
				return hasBaseDelegateParameter
					? parameters.Length != 1
					: !parameters.IsDefaultOrEmpty;
			}

			switch (method.ReturnType.SpecialType)
			{
				case SpecialType.System_Array:
				case SpecialType.System_Collections_IEnumerable:
				case SpecialType.System_Collections_Generic_IEnumerable_T:
				case SpecialType.System_Collections_Generic_IList_T:
				case SpecialType.System_Collections_Generic_ICollection_T:
				case SpecialType.System_Collections_Generic_IReadOnlyList_T:
				case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
				case SpecialType.System_Collections_IEnumerator:
				case SpecialType.System_Collections_Generic_IEnumerator_T:
					if (parameters.IsDefaultOrEmpty || !parameters[0].Type.Equals(pxContext.PXAdapterType, SymbolEqualityComparer.Default))
						return true;
					else
					{
						return hasBaseDelegateParameter
							? parameters.Length < 2
							: false;
					}
				default:
					return true;
			}
		}
	}
}