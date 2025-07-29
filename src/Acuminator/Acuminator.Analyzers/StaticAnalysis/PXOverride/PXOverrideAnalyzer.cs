using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	public class PXOverrideAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1096_PXOverrideMustMatchSignature);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphExtension) =>
			base.ShouldAnalyze(pxContext, graphExtension) && graphExtension.GraphType == GraphType.PXGraphExtension &&
			!graphExtension.PXOverrides.IsDefaultOrEmpty;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var directBaseTypesAndThis = graphExtension.Symbol.GetBaseTypesAndThis().ToList(capacity: 4);

			var allGraphAndGraphExtensionBaseTypes = 
				graphExtension.Symbol.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending, includeGraph: true)
									.SelectMany(t => t.GetBaseTypesAndThis())
									.OfType<INamedTypeSymbol>()
									.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
									.Where(baseType => !directBaseTypesAndThis.Contains(baseType, SymbolEqualityComparer.Default))
									.ToList();

			foreach (PXOverrideInfo pxOverrideInfo in graphExtension.PXOverrides)
			{
				AnalyzePatchMethod(context, pxContext, allGraphAndGraphExtensionBaseTypes, pxOverrideInfo);
			}
		}

		private void AnalyzePatchMethod(SymbolAnalysisContext context, PXContext pxContext, List<INamedTypeSymbol> allGraphAndGraphExtensionBaseTypes,
										PXOverrideInfo pxOverrideInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			context.CancellationToken.ThrowIfCancellationRequested();

			if (!CheckSignatureCompatibility(context, pxContext, allGraphAndGraphExtensionBaseTypes, pxOverrideInfo.Symbol))
				return;
		}

		private static bool CheckSignatureCompatibility(SymbolAnalysisContext context, PXContext pxContext,
														List<INamedTypeSymbol> allGraphAndGraphExtensionBaseTypes,
														IMethodSymbol patchMethodWithPXOverride)
		{
			if (!patchMethodWithPXOverride.IsStatic && !patchMethodWithPXOverride.IsGenericMethod)
			{
				foreach (var baseType in allGraphAndGraphExtensionBaseTypes)
				{
					bool hasSuitablePXOverride = baseType.GetMethods(patchMethodWithPXOverride.Name)
														 .Any(m => patchMethodWithPXOverride.IsPXOverrideOf(m));
					if (hasSuitablePXOverride)
						return true;
				}
			}

			var location = patchMethodWithPXOverride.Locations.FirstOrDefault();

			if (location != null)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}

			return false;
		}
	}
}
