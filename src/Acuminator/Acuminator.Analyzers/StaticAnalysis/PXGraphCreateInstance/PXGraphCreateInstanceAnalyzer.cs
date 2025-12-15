using System.Collections.Immutable;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class PXGraphCreateInstanceAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1001_PXGraphCreateInstance,
				Descriptors.PX1003_BasePXGraphCreateInstance);

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
