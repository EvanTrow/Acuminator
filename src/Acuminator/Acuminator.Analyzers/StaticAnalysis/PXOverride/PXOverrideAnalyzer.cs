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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual,
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter,
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter
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
			CheckPatchMethodBaseDelegateParameter(context, pxContext, pxOverrideInfo);

			context.CancellationToken.ThrowIfCancellationRequested();

			if (pxOverrideInfo.BaseMethod == null)
			{
				ReportPatchMethodWithIncompatibleSignature(context, pxContext, pxOverrideInfo.Symbol);
			}
			else
			{
				CheckPatchMethodForXmlDocComment(context, pxContext, pxOverrideInfo);
			}
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
					{ PXOverrideDiagnosticProperties.PatchMethodName,           patchMethodWithPXOverride.Name }
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

		protected virtual void CheckPatchMethodBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext, PXOverrideInfo pxOverrideInfo)
		{
			DiagnosticDescriptor descriptor;
			Location? location;
			BaseDelegateParameterFixMode fixMode;
			bool registerDiagnostic;

			switch (pxOverrideInfo.OverrideType)
			{
				case PXOverrideType.WithoutBaseDelegate:
					descriptor = Descriptors.PX1079_PXOverrideWithoutDelegateParameter;
					location = pxOverrideInfo.Symbol.Locations.FirstOrDefault();
					fixMode = BaseDelegateParameterFixMode.AddDelegateParameter;
					registerDiagnostic = !pxOverrideInfo.SignatureHasNonTrivialRefKind;
					break;

				case PXOverrideType.WithInvalidBaseDelegate:
					descriptor = Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter;
					location = GetLocationForIncorrectDelegateParameter(pxOverrideInfo.Symbol, context.CancellationToken);
					fixMode = BaseDelegateParameterFixMode.ReplaceDelegateParameter;
					registerDiagnostic = !pxOverrideInfo.SignatureHasNonTrivialRefKind;
					break;

				case PXOverrideType.WithValidBaseDelegate
				when !IsCorrectDelegateParameterName(pxOverrideInfo.Symbol):
					descriptor = Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter;
					location = GetLocationForDelegateParameterWithIncorrectName(pxOverrideInfo.Symbol, context.CancellationToken);
					fixMode = BaseDelegateParameterFixMode.RenameDelegateParameter;
					registerDiagnostic = true;
					break;

				default:
					return;
			}

			var diagnosticProperties = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
			{
				{ DiagnosticProperty.RegisterCodeFix, registerDiagnostic.ToString() },
				{ PXOverrideDiagnosticProperties.PatchMethodName, pxOverrideInfo.Symbol.Name },
				{ PXOverrideDiagnosticProperties.DelegateParameterFixMode, fixMode.ToString() }
			}
			.ToImmutableDictionary();

			var diagnostic = Diagnostic.Create(descriptor, location, diagnosticProperties);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private Location? GetLocationForIncorrectDelegateParameter(IMethodSymbol patchMethodWithPXOverride, CancellationToken cancellation)
		{
			var parameters = patchMethodWithPXOverride.Parameters;

			if (parameters.IsDefaultOrEmpty)
				return patchMethodWithPXOverride.Locations.FirstOrDefault();

			if (patchMethodWithPXOverride.GetSyntax(cancellation) is not MethodDeclarationSyntax methodNode)
				return patchMethodWithPXOverride.Locations.FirstOrDefault();

			var lastParameterNode = methodNode.ParameterList.Parameters[^1];
			return lastParameterNode.GetLocation().NullIfLocationKindIsNone() ??
				   parameters[^1].Locations.FirstOrDefault()?.NullIfLocationKindIsNone() ??
				   patchMethodWithPXOverride.Locations.FirstOrDefault();
		}

		private bool IsCorrectDelegateParameterName(IMethodSymbol patchMethodWithPXOverride)
		{
			if (patchMethodWithPXOverride.Parameters.IsDefaultOrEmpty)
				return true;

			var lastParameter = patchMethodWithPXOverride.Parameters[^1];
			string properName = $"base_{patchMethodWithPXOverride.Name}";

			return lastParameter.Name.Equals(properName, StringComparison.OrdinalIgnoreCase);
		}

		private Location? GetLocationForDelegateParameterWithIncorrectName(IMethodSymbol patchMethodWithPXOverride, CancellationToken cancellation)
		{
			var parameters = patchMethodWithPXOverride.Parameters;

			if (parameters.IsDefaultOrEmpty)
				return patchMethodWithPXOverride.Locations.FirstOrDefault();

			return parameters[^1].Locations.FirstOrDefault()?.NullIfLocationKindIsNone() ??
				   patchMethodWithPXOverride.Locations.FirstOrDefault();
		}

		protected virtual void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																		  IMethodSymbol patchMethodWithPXOverride)
		{
			var location = patchMethodWithPXOverride.Locations.FirstOrDefault();
			var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		protected virtual void CheckPatchMethodForXmlDocComment(SymbolAnalysisContext context, PXContext pxContext, PXOverrideInfo pxOverrideInfo)
		{
			if (pxOverrideInfo.BaseMethod == null || pxOverrideInfo.OverrideType is (PXOverrideType.WithoutBaseDelegate or PXOverrideType.None))
				return;

			context.CancellationToken.ThrowIfCancellationRequested();

			if (pxOverrideInfo.Symbol.GetSyntax(context.CancellationToken) is not MethodDeclarationSyntax patchMethodNode)
				return;

			var semanticModel = context.Compilation.GetSemanticModel(patchMethodNode.SyntaxTree);

			if (semanticModel == null || 
				BaseMethodReferenceInXmlDocComment.HasCorrectReference(semanticModel, patchMethodNode, pxOverrideInfo.BaseMethod, context.CancellationToken))
			{
				return;
			}

			var location = patchMethodNode.Identifier.GetLocation().NullIfLocationKindIsNone() ?? 
						   pxOverrideInfo.Symbol.Locations.FirstOrDefault();
			var baseMethodDocCommentID = GetPreparedTextWithReferenceToBaseAPI(pxOverrideInfo.BaseMethod);
			ImmutableDictionary<string, string?> diagnosticProperties;

			if (baseMethodDocCommentID != null)
			{
				diagnosticProperties = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
				{
					{ PXOverrideDiagnosticProperties.BaseMethodDocCommentId, baseMethodDocCommentID }
				}
				.ToImmutableDictionary();
			}
			else
				diagnosticProperties = ImmutableDictionary<string, string?>.Empty;

			var diagnostic = Diagnostic.Create(Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment, location, diagnosticProperties);
			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static string? GetPreparedTextWithReferenceToBaseAPI(IMethodSymbol method)
		{
			ISymbol? symbolToGetDocID = method.MethodKind switch
			{
				MethodKind.ReducedExtension 					 					   => method.ReducedFrom,
				MethodKind.PropertyGet or MethodKind.PropertySet 					   => method.AssociatedSymbol,
				MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise => method.AssociatedSymbol,
				_ 																	   => method
			};

			string? docCommentID = symbolToGetDocID?.GetDocumentationCommentId().NullIfWhiteSpace();
			docCommentID = docCommentID?.Length > 2
				? docCommentID.Substring(2)				// Remove "M:" and other API type prefixes
				: null;

			return docCommentID != null
				? docCommentID.Replace(",", ", ")		// make parameters list more readable and look like the one VS inserts
				: null;
		}
	}
}
