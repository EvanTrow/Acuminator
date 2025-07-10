using System.Collections.Immutable;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXGraphCreateInstanceAnalyzer : PXDiagnosticAnalyzer
	{
		private class Walker : CSharpSyntaxWalker
		{
			private readonly SyntaxNodeAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;

			private static readonly DiagnosticDescriptor _px1001Descriptor = Descriptors.PX1001_PXGraphCreateInstance;
			private static readonly DiagnosticDescriptor _px1003Descriptor = Descriptors.PX1003_NonSpecificPXGraphCreateInstance;

			public Walker(SyntaxNodeAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
			{
				_context = context;
				_pxContext = pxContext;
				_semanticModel = semanticModel;
			}

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				if (node.Type == null || _semanticModel.GetSymbolInfo(node.Type).Symbol is not ITypeSymbol typeSymbol)
				{
					base.VisitObjectCreationExpression(node);
					return;
				}

				DiagnosticDescriptor? descriptor = GetDiagnosticDescriptor(typeSymbol);

				if (descriptor != null)
				{
					_context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, node.GetLocation()),
						_pxContext.CodeAnalysisSettings);
				}

				base.VisitObjectCreationExpression(node);
			}

			private DiagnosticDescriptor? GetDiagnosticDescriptor(ITypeSymbol typeSymbol)
			{
				if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.IsPXGraph(_pxContext))
				{
					return _px1001Descriptor;
				}
				else if (typeSymbol.InheritsFrom(_pxContext.PXGraph.Type))
				{
					return _px1001Descriptor;
				}
				else if (typeSymbol.Equals(_pxContext.PXGraph.Type, SymbolEqualityComparer.Default))
				{
					return _px1003Descriptor;
				}

				return null;
			}
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1001_PXGraphCreateInstance,
				Descriptors.PX1003_NonSpecificPXGraphCreateInstance);

		public PXGraphCreateInstanceAnalyzer() : this(null)
		{ }

		public PXGraphCreateInstanceAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeGraphCreation(context, pxContext), 
															 SyntaxKind.CompilationUnit);
		}

		private void AnalyzeGraphCreation(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Node is CompilationUnitSyntax compilationUnit)
			{
				var walker = new Walker(context, pxContext, context.SemanticModel);
				compilationUnit.Accept(walker);
			}
		}
	}
}
