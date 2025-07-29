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

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
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

			var allBaseTypes = graphExtension.Symbol
				.GetGraphExtensionWithBaseExtensions(pxContext, SortDirection.Ascending, includeGraph: true)
				.SelectMany(t => t.GetBaseTypesAndThis())
				.OfType<INamedTypeSymbol>()
				.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
				.Where(baseType => !directBaseTypesAndThis.Contains(baseType, SymbolEqualityComparer.Default))
				.ToList();

			foreach (PXOverrideInfo pxOverride in graphExtension.PXOverrides)
			{
				AnalyzeMethod(context, pxContext, allBaseTypes, pxOverride.Symbol);
			}
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, List<INamedTypeSymbol> allBaseTypes, 
								   IMethodSymbol methodWithPXOverride)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!methodWithPXOverride.IsStatic && !methodWithPXOverride.IsGenericMethod)
			{
				foreach (var baseType in allBaseTypes)
				{
					bool hasSuitablePXOverride = baseType.GetMethods(methodWithPXOverride.Name)
														 .Any(m => methodWithPXOverride.IsPXOverrideOf(m));
					if (hasSuitablePXOverride)
						return;
				}
			}

			var location = methodWithPXOverride.Locations.FirstOrDefault();

			if (location != null)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}
	}
}
