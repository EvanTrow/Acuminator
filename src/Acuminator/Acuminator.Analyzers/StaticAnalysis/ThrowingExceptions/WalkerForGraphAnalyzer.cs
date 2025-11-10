using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
	internal class WalkerForGraphAnalyzer : WalkerBase
	{
		private readonly DiagnosticDescriptor _descriptor;

		public WalkerForGraphAnalyzer(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor) : base(context, pxContext)
		{
			_descriptor = descriptor.CheckIfNull();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void VisitProcessingDelegate(ProcessingDelegateInfo? processingDelegateInfo) =>
			VisitDelegateNode(processingDelegateInfo?.Node, processingDelegateInfo?.Symbol);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void VisitLongRunDelegate(SyntaxNode? delegateNode, ISymbol? delegateSymbol) =>
			VisitDelegateNode(delegateNode, delegateSymbol);

		private void VisitDelegateNode(SyntaxNode? delegateNode, ISymbol? delegateSymbol)
		{
			if (delegateNode == null || delegateSymbol == null)
				return;

			if (delegateNode is NameSyntax methodIdentifier && delegateSymbol is IMethodSymbol method)
			{
				VisitCalledMethod(method, methodIdentifier);
			}
			else
			{
				Visit(delegateNode);
			}
		}

		public override void VisitThrowExpression(ThrowExpressionSyntax throwExpression)
		{
			ThrowIfCancellationRequested();

			if (IsPXSetupNotEnteredException(throwExpression.Expression))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, throwExpression);
			}
			else
			{
				base.VisitThrowExpression(throwExpression);
			}
		}

		public override void VisitThrowStatement(ThrowStatementSyntax throwStatement)
		{
			ThrowIfCancellationRequested();

			if (IsPXSetupNotEnteredException(throwStatement.Expression))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, throwStatement);
			}
			else
			{
				base.VisitThrowStatement(throwStatement);
			}
		}
	}
}
