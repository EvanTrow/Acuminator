using System;
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
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter,
				Descriptors.PX1096_PXOverrideMustMatchSignature,
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual
			);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphExtension) =>
			base.ShouldAnalyze(pxContext, graphExtension) && graphExtension.GraphType == GraphType.PXGraphExtension &&
			!graphExtension.DeclaredPXOverrides.IsDefaultOrEmpty;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			foreach (PXOverrideInfo pxOverrideInfo in graphExtension.DeclaredPXOverrides)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				// We do not report generic PXOverrides. Although they are not supported now they can be supported in the future.
				if (!pxOverrideInfo.Symbol.IsGenericMethod)
				{
					AnalyzePatchMethod(context, pxContext, pxOverrideInfo);
				}
			}
		}

		private void AnalyzePatchMethod(SymbolAnalysisContext context, PXContext pxContext, PXOverrideInfo pxOverrideInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CheckPatchMethodIsPublicNonVirtual(context, pxContext, pxOverrideInfo.Symbol);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckPatchMethodHasBaseDelegateParameter(context, pxContext, pxOverrideInfo);

			context.CancellationToken.ThrowIfCancellationRequested();

			if (pxOverrideInfo.BaseMethod == null)
				ReportPatchMethodWithIncompatibleSignature(context, pxContext, pxOverrideInfo.Symbol);
		}

		protected virtual void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext, 
																  IMethodSymbol patchMethodWithPXOverride)
		{
			bool isNonPublic = patchMethodWithPXOverride.DeclaredAccessibility != Accessibility.Public;
			var virtualityKind = GetPatchMethodVirtualityKind(patchMethodWithPXOverride);

			if (isNonPublic || virtualityKind != MemberVirtualityKind.None)
			{
				var location = patchMethodWithPXOverride.Locations.FirstOrDefault();
				var diagnosticProperties = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
				{
					{ PXOverrideDiagnosticProperties.IsNonPublicPatchMethod,    isNonPublic.ToString() },
					{ PXOverrideDiagnosticProperties.PatchMethodVirtualityKind, virtualityKind.ToString() },
					{ PXOverrideDiagnosticProperties.PatchMethodName,			patchMethodWithPXOverride.Name }
				}
				.ToImmutableDictionary();

				var diagnostic = Diagnostic.Create(Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual, location, diagnosticProperties);
				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private MemberVirtualityKind GetPatchMethodVirtualityKind(IMethodSymbol patchMethodWithPXOverride)
		{
			if (patchMethodWithPXOverride.IsVirtual)
				return MemberVirtualityKind.Virtual;
			else if (patchMethodWithPXOverride.IsAbstract)
				return MemberVirtualityKind.Abstract;
			else if (patchMethodWithPXOverride.IsOverride)
				return patchMethodWithPXOverride.IsSealed 
					? MemberVirtualityKind.SealedOverride 
					: MemberVirtualityKind.Override;
			else
				return MemberVirtualityKind.None;
		}

		protected virtual void CheckPatchMethodHasBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext, PXOverrideInfo pxOverrideInfo)
		{
			if (pxOverrideInfo.OverrideType == PXOverrideType.WithoutBaseDelegate)
			{
				var location = pxOverrideInfo.Symbol.Locations.FirstOrDefault();
				var diagnostic = Diagnostic.Create(Descriptors.PX1079_PXOverrideWithoutDelegateParameter, location);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		protected virtual void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																		  IMethodSymbol patchMethodWithPXOverride)
		{
			var location = patchMethodWithPXOverride.Locations.FirstOrDefault();
			var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}
