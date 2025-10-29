
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisDac
{
    public class DacAndDacExtensionDeclarationAnalyzer : DacAggregatedAnalyzerBase
	{
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create
			(
				Descriptors.PX1009_InheritanceFromDacExtension,
				Descriptors.PX1011_NotSealedDacExtension
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (dacOrDacExtension.DacType == DacType.DacExtension)
			{
				CheckIfDacExtensionForInheritanceIssues(context, pxContext, dacOrDacExtension);
			}
		}

		private void CheckIfDacExtensionForInheritanceIssues(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
		{
			if (dacExtension.Name == TypeNames.PXCacheExtension || dacExtension.IsMappedCacheExtension)
				return;

			var baseType = dacExtension.Symbol.BaseType;

			if (baseType != null && baseType.IsDacExtensionBaseType())
			{
				CheckIfDacExtensionIsSealed(context, pxContext, dacExtension);
			}
			else
			{
				ReportDacExtensionInheritance(context, pxContext, dacExtension);
			}
		}

		protected virtual void CheckIfDacExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
		{
			if (!dacExtension.Symbol.IsSealed)
			{
				var location = dacExtension.Symbol.Locations.FirstOrDefault();
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1011_NotSealedDacExtension, location),
					pxContext.CodeAnalysisSettings);
			}
		}

		protected virtual void ReportDacExtensionInheritance(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
		{
			var location = dacExtension.Symbol.Locations.FirstOrDefault();
			context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1009_InheritanceFromDacExtension, location),
					pxContext.CodeAnalysisSettings);
		}
	}
}
