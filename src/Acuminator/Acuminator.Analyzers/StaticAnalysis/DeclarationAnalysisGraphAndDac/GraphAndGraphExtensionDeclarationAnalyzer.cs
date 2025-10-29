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
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraphAndDac
{
	public class GraphAndGraphExtensionDeclarationAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1093_GraphDeclarationViolation,
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract,
				Descriptors.PX1113_SealedGraphsAndGraphExtensions,
				Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension,
				Descriptors.PX1115_NonTerminalBaseGraphExtension
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CheckIfGraphOrGraphExtensionIsGenericNonAbstract(context, pxContext, graphOrGraphExt);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckIfGraphOrGraphExtensionIsSealed(context, pxContext, graphOrGraphExt);

			context.CancellationToken.ThrowIfCancellationRequested();
			var semanticModel = context.Compilation.GetSemanticModel(graphOrGraphExt.Node!.SyntaxTree);

			if (graphOrGraphExt.GraphType == GraphType.PXGraph)
			{
				CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(context, pxContext, semanticModel, graphOrGraphExt);
			}
			else
			{
				CheckIfGraphExtensionInheritsFromNonAbstractGraphExtension(context, pxContext, semanticModel, graphOrGraphExt);
			}
		}

		protected virtual void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																				PXGraphEventSemanticModel graphOrGraphExt)
		{
			if (graphOrGraphExt.Symbol.IsAbstract || !graphOrGraphExt.Symbol.IsGenericType ||
				graphOrGraphExt.Symbol.TypeParameters.IsDefaultOrEmpty)
			{
				return;
			}
			
			var location = graphOrGraphExt.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ??
						   graphOrGraphExt.Node.GetLocation();
			var diagnostic = Diagnostic.Create(
											Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract, location);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		protected virtual void CheckIfGraphOrGraphExtensionIsSealed(SymbolAnalysisContext context, PXContext pxContext,
																	PXGraphEventSemanticModel graphOrGraphExt)
		{
			if (!graphOrGraphExt.Symbol.IsSealed)
				return;

			var sealedToken = graphOrGraphExt.Node!.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.SealedKeyword));
			var location = sealedToken != default
				? sealedToken.GetLocation().NullIfLocationKindIsNone()
				: null;

			location ??= graphOrGraphExt.Node.Identifier.GetLocation().NullIfLocationKindIsNone() ??
						 graphOrGraphExt.Node.GetLocation();
			var diagnostic = Diagnostic.Create(
											Descriptors.PX1113_SealedGraphsAndGraphExtensions, location);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		protected virtual void CheckIfBaseGraphTypeSpecifyCorrectGraphAsTypeArgument(SymbolAnalysisContext context, PXContext pxContext,
																					 SemanticModel? semanticModel, PXGraphEventSemanticModel graph)
		{
			var graphArgumentNode = GetGraphTypeArgumentNodeFromBaseGraphType(semanticModel, pxContext, graph.Node!, context.CancellationToken);

			if (graphArgumentNode == null)
				return;

			// Get last identifier to handle cases like SO.SOSetupMaint
			var graphArgumentIdentifier = graphArgumentNode.DescendantNodesAndSelf()
														   .OfType<IdentifierNameSyntax>()
														   .LastOrDefault();
			if (graphArgumentIdentifier == null)
				return;

			var graphTypeArgumentTypeInfo = semanticModel.GetTypeInfo(graphArgumentIdentifier, context.CancellationToken);
			var graphTypeArgumentType = graphTypeArgumentTypeInfo.Type;

			if (graphTypeArgumentType?.TypeKind != TypeKind.Class || graph.Symbol.Equals(graphTypeArgumentType, SymbolEqualityComparer.Default))
			{
				return;
			}

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1093_GraphDeclarationViolation, graphArgumentIdentifier.GetLocation()),
				pxContext.CodeAnalysisSettings);
		}

		private TypeSyntax? GetGraphTypeArgumentNodeFromBaseGraphType(SemanticModel? semanticModel, PXContext pxContext,
																	  ClassDeclarationSyntax graphNode, CancellationToken cancellation)
		{
			var baseGraphTypeInfo = semanticModel != null
				? GraphSyntaxUtils.GetBaseGraphTypeInfo(semanticModel, pxContext, graphNode, cancellation)
				: null;

			if (baseGraphTypeInfo == null)
				return null;

			var (baseTypeSymbol, baseTypeNode) = baseGraphTypeInfo.Value;

			if (!baseTypeSymbol.IsGraphBaseType() || baseTypeSymbol.TypeArguments.IsDefaultOrEmpty)
				return null;

			var typeArgumentsListNode = baseTypeNode.DescendantNodes()
													.OfType<TypeArgumentListSyntax>()
													.FirstOrDefault();

			return typeArgumentsListNode?.Arguments.FirstOrDefault();
		}

		protected virtual void CheckIfGraphExtensionInheritsFromNonAbstractGraphExtension(SymbolAnalysisContext context, PXContext pxContext,
																			SemanticModel? semanticModel, PXGraphEventSemanticModel graphExtension)
		{
			bool isDerivedFromTerminalGraphExtension = 
				graphExtension.Symbol.GetGraphExtensionBaseTypes()
									 .OfType<INamedTypeSymbol>()
									 .Any(baseExtension => baseExtension.IsTerminalGraphExtension(pxContext));

			if (!isDerivedFromTerminalGraphExtension)
				return;

			var location = GetDiagnosticLocation();
			var diagnostic = Diagnostic.Create(
											Descriptors.PX1114_GraphExtensionInheritFromNonAbstractGraphExtension, location);
			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);

			//-----------------------------------------------Local Function------------------------------------------------
			Location? GetDiagnosticLocation()
			{
				if (semanticModel == null)
				{
					return graphExtension.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ??
						   graphExtension.Node.GetLocation();
				}

				var baseGraphExtensionInfo = GraphSyntaxUtils.GetBaseGraphExtensionTypeInfo(semanticModel, pxContext, graphExtension.Node,
																							context.CancellationToken);
				var location = baseGraphExtensionInfo?.TypeNode.GetLocation().NullIfLocationKindIsNone();
				location ??= graphExtension.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ??
							 graphExtension.Node.GetLocation();
				return location;
			}
		}
	}
}