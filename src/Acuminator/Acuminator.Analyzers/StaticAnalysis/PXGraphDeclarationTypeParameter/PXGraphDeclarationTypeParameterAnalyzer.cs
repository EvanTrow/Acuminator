
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphDeclarationTypeParameter
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXGraphDeclarationTypeParameterAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1093_GraphDeclarationViolation);

		public PXGraphDeclarationTypeParameterAnalyzer() : this(null)
		{ }

		public PXGraphDeclarationTypeParameterAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphDeclarationTypeParameter(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeGraphDeclarationTypeParameter(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Node is not ClassDeclarationSyntax classDeclaration)
			{
				return;
			}

			var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
			if (typeSymbol == null || !typeSymbol.IsPXGraph(pxContext))
			{
				return;
			}

			var graphArgumentNode = GetBaseGraphTypeNode(context, pxContext, classDeclaration);
			if (graphArgumentNode == null)
			{
				return;
			}

			// Get last identifier to handle cases like SO.SOSetupMaint
			var graphArgumentIdentifier = graphArgumentNode
				.DescendantNodesAndSelf()
				.OfType<IdentifierNameSyntax>()
				.Last();

			var graphTypeArgument = context.SemanticModel.GetTypeInfo(graphArgumentIdentifier).Type;

			if (typeSymbol.Equals(graphTypeArgument, SymbolEqualityComparer.Default) || graphTypeArgument?.Kind == SymbolKind.TypeParameter)
			{
				return;
			}

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1093_GraphDeclarationViolation, graphArgumentIdentifier.GetLocation()),
				pxContext.CodeAnalysisSettings);
		}

		private TypeSyntax? GetBaseGraphTypeNode(SyntaxNodeAnalysisContext context, PXContext pxContext,
												 ClassDeclarationSyntax graphNode)
		{
			var baseGraphTypeInfo = GraphSyntaxUtils.GetBaseGraphTypeInfo(context.SemanticModel, pxContext, graphNode, 
																		  context.CancellationToken);
			if (baseGraphTypeInfo == null)
				return null;

			var (baseTypeSymbol, baseTypeNode) = baseGraphTypeInfo.Value;
			var isGraphBaseType = baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraph, SymbolEqualityComparer.Default) ||
								  baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraphDac, SymbolEqualityComparer.Default) ||
								  baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraphDacField, SymbolEqualityComparer.Default);

			if (!isGraphBaseType)
				return null;

			var typeArgumentsListNode = baseTypeNode.DescendantNodes()
													.OfType<TypeArgumentListSyntax>()
													.FirstOrDefault();

			return typeArgumentsListNode?.Arguments.FirstOrDefault();
		}
	}
}
