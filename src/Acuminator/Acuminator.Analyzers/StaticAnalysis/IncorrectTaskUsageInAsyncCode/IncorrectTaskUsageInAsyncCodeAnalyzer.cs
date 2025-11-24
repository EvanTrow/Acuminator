using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.IncorrectTaskUsageInAsyncCode
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class IncorrectTaskUsageInAsyncCodeAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask
			);

		public IncorrectTaskUsageInAsyncCodeAnalyzer() : this(null)
		{ }

		public IncorrectTaskUsageInAsyncCodeAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override bool ShouldAnalyze(PXContext pxContext) => 
			base.ShouldAnalyze(pxContext) && 
			pxContext.AsyncOperations.Task != null && pxContext.AsyncOperations.Task_Generic != null;

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeTaskUsageInTypes(c, pxContext), SyntaxKind.CompilationUnit);
		}

		private static void AnalyzeTaskUsageInTypes(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not CompilationUnitSyntax compilationUnit)
				return;

			var tasksChecker	 = new TaskUsageCheckingWalker(syntaxContext, pxContext);
			var typeDeclarations = compilationUnit.DescendantNodes()
												  .OfType<TypeDeclarationSyntax>();

			foreach (TypeDeclarationSyntax typeNode in typeDeclarations)
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();
				tasksChecker.Visit(typeNode);
			}
		}
	}
}