using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.IncorrectTaskUsageInAsyncCode
{
	public partial class IncorrectTaskUsageInAsyncCodeAnalyzer : PXDiagnosticAnalyzer
	{
		private class TaskUsageCheckingWalker : CSharpSyntaxWalker 
		{
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private readonly PXContext _pxContext;

			private readonly INamedTypeSymbol? _valueTaskType;
			private readonly INamedTypeSymbol? _valueTaskGenericType;

			public CancellationToken Cancellation => _syntaxContext.CancellationToken;

			public SemanticModel SemanticModel => _syntaxContext.SemanticModel;

			public TaskUsageCheckingWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
			{
				_syntaxContext 		  = syntaxContext;
				_pxContext	   		  = pxContext;
				_valueTaskType 		  = _pxContext.AsyncOperations.ValueTask;
				_valueTaskGenericType = _pxContext.AsyncOperations.ValueTask_Generic;
			}

			#region Visitor Optimization - do not visit some subtrees
			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node) { }

			public override void VisitEnumDeclaration(EnumDeclarationSyntax node) { }

			public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node) { }

			public override void VisitXmlComment(XmlCommentSyntax node) { }
			#endregion

			public override void VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
			{
				var variableType = SemanticModel.GetSymbolOrFirstCandidate(variableDeclaration.Type, Cancellation) as ITypeSymbol;

				if (variableType == null || !IsTaskType(variableType))
				{
					base.VisitVariableDeclaration(variableDeclaration);
					return;
				}

				var location   = variableDeclaration.Type.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
				base.VisitVariableDeclaration(variableDeclaration);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression)
			{
				Cancellation.ThrowIfCancellationRequested();

				// Do a cheaper check for awaited invocations first to avoid semantic model query
				if (invocationExpression.Parent is AwaitExpressionSyntax)
				{
					base.VisitInvocationExpression(invocationExpression);
					return;
				}

				var invocationType = SemanticModel.GetTypeInfo(invocationExpression, Cancellation).Type;

				if (invocationType == null || invocationType.SpecialType != SpecialType.None || !IsTaskType(invocationType))
				{
					base.VisitInvocationExpression(invocationExpression);
					return;
				}

				var location = invocationExpression.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
				base.VisitInvocationExpression(invocationExpression);
			}

			public override void VisitReturnStatement(ReturnStatementSyntax returnStatement)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(returnStatement.Expression);
				base.VisitReturnStatement(returnStatement);
			}

			public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax arrowExpressionMethodBody)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(arrowExpressionMethodBody.Expression);
				base.VisitArrowExpressionClause(arrowExpressionMethodBody);
			}

			private void CheckTypeMemberReturningTaskHasTaskReturnType(ExpressionSyntax? returnExpression)
			{
				if (returnExpression == null)
					return;

				var returnExpressionType = SemanticModel.GetTypeInfo(returnExpression, Cancellation).Type;

				if (returnExpressionType == null || !IsTaskType(returnExpressionType))
					return;

				var containingMethodOrLocalFunction = returnExpression.GetContainingMemberOrLocalFunction();
				var returnTypeNode = containingMethodOrLocalFunction switch
				{
					MethodDeclarationSyntax methodDeclaration	  => methodDeclaration.ReturnType,
					PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type,
					LocalFunctionStatementSyntax localFunction	  => localFunction.ReturnType,
					_ 											  => null
				};

				if (returnTypeNode == null)
					return;

				var returnTypeSymbol = SemanticModel.GetSymbolOrFirstCandidate(returnTypeNode, Cancellation) as ITypeSymbol;

				if (returnTypeSymbol == null || IsTaskType(returnTypeSymbol))
					return;

				var location   = returnExpression.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
			}

			private bool IsTaskType(ITypeSymbol typeSymbol) =>
				typeSymbol.Equals(_pxContext.AsyncOperations.Task, SymbolEqualityComparer.Default) ||
				typeSymbol.OriginalDefinition.Equals(_pxContext.AsyncOperations.Task_Generic, SymbolEqualityComparer.Default) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol, _valueTaskType) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _valueTaskGenericType);
		}
	}
}