using System.Threading;

using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public partial class DatabaseQueriesInRowSelectingAnalyzer
	{
		private partial class DiagnosticWalker : Walker
		{

			private readonly PXConnectionScopeVisitor _connectionScopeVisitor;
			private bool _insideConnectionScope;

			public DiagnosticWalker(SymbolAnalysisContext context, PXContext pxContext)
				: base(context, pxContext, Descriptors.PX1042_DatabaseQueriesInRowSelecting)
			{
				_connectionScopeVisitor = new PXConnectionScopeVisitor(this, pxContext);
			}

			public override void VisitUsingStatement(UsingStatementSyntax node)
			{
				ThrowIfCancellationRequested();

				if (_insideConnectionScope)
				{
					base.VisitUsingStatement(node);
				}
				else
				{
					try
					{
						_insideConnectionScope = node.Accept(_connectionScopeVisitor);
						base.VisitUsingStatement(node);
					}
					finally
					{
						_insideConnectionScope = false;
					}
				}
			}

			public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
			{
				// This method supports expressions like "using var x = new PXConnectionScope();"
				ThrowIfCancellationRequested();

				if (node.UsingKeyword == default || !node.UsingKeyword.IsKind(SyntaxKind.UsingKeyword) || _insideConnectionScope)
				{
					base.VisitLocalDeclarationStatement(node);
					return;
				}

				_insideConnectionScope = node.Accept(_connectionScopeVisitor);
				base.VisitLocalDeclarationStatement(node);
				_insideConnectionScope = false;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				if (!_insideConnectionScope)
					base.VisitInvocationExpression(node);
			}
		}
	}
}
