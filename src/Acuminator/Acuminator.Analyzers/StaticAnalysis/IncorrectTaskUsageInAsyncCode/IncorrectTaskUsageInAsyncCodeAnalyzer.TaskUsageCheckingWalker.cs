using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

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

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				Cancellation.ThrowIfCancellationRequested();
				


				base.VisitInvocationExpression(node);
			}

			private bool IsTaskType(ITypeSymbol typeSymbol) =>
				typeSymbol.Equals(_pxContext.AsyncOperations.Task, SymbolEqualityComparer.Default) ||
				typeSymbol.OriginalDefinition.Equals(_pxContext.AsyncOperations.Task_Generic, SymbolEqualityComparer.Default) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol, _valueTaskType) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _valueTaskGenericType);
		}
	}
}