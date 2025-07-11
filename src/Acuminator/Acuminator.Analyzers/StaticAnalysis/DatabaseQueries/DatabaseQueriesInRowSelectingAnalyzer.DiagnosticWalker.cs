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
				_connectionScopeVisitor = new PXConnectionScopeVisitor(this, pxContext, 
														semanticModelGetter: GetSemanticModel, 
														CancellationToken);
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
				// Expressions like "using var x = new PXConnectionScope();" are not supported and won't be supported
				// since PX1042 diagnostic is obsolete and works only for Acumatica 2022R2 and older.
				// No reason to invest into it.
				ThrowIfCancellationRequested();
				base.VisitLocalDeclarationStatement(node);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				if (!_insideConnectionScope)
					base.VisitInvocationExpression(node);
			}
		}
	}
}
