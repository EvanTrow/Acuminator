using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension
{
	public class ConstructorInGraphExtensionAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphExtension) =>
			base.ShouldAnalyze(pxContext, graphExtension) && graphExtension.GraphType == GraphType.PXGraphExtension &&
			graphExtension.Symbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.InstanceConstructors.IsDefaultOrEmpty && 
			!namedTypeSymbol.IsGraphExtensionBaseType();

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (graphExtension.Symbol is not INamedTypeSymbol graphExtensionType)
				return;

			var constructorLocations = graphExtensionType.InstanceConstructors.Where(constructor => !constructor.IsImplicitlyDeclared)
																			  .SelectMany(constructor => constructor.Locations);
			foreach (Location location in constructorLocations)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1040_ConstructorInGraphExtension, location), 
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}
