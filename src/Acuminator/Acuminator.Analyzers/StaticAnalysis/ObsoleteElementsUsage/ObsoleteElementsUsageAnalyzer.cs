using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ObsoleteElementsUsage
{
	public class ObsoleteElementsUsageAnalyzer : IPXGraphAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = new[] 
		{ 
			Descriptors.PX1100_PXOverrideOverridesObsoleteMethod 
		}
		.Distinct()
		.ToImmutableArray();

		public bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphExtension) =>
			graphExtension != null && graphExtension.IsInSource && 
			pxContext.AttributeTypes.ObsoleteAttribute != null &&
			graphExtension.GraphType == GraphType.PXGraphExtension;

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var obsoleteAttribute = pxContext.AttributeTypes.ObsoleteAttribute;
			CheckPXOverrideMethods(context, pxContext, graphExtension, obsoleteAttribute);
		}

		private void CheckPXOverrideMethods(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension, 
											INamedTypeSymbol obsoleteAttribute)
		{
			foreach (PXOverrideInfo pxOverrideInfo in graphExtension.DeclaredPXOverrides)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				CheckPXOverrideMethod(context, pxContext, pxOverrideInfo, obsoleteAttribute);
			}
		}

		private void CheckPXOverrideMethod(SymbolAnalysisContext context, PXContext pxContext, PXOverrideInfo pxOverrideInfo,
											INamedTypeSymbol obsoleteAttribute)
		{
			if (pxOverrideInfo.BaseMethod == null)
				return;

			var attributes = pxOverrideInfo.BaseMethod.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return;

			bool hasObsoleteAttribute = attributes.Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, obsoleteAttribute));

			if (hasObsoleteAttribute)
			{
				var location = pxOverrideInfo.Symbol.Locations.FirstOrDefault();
				context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1100_PXOverrideOverridesObsoleteMethod, location),
						pxContext.CodeAnalysisSettings);
			}
		}
	}
}
