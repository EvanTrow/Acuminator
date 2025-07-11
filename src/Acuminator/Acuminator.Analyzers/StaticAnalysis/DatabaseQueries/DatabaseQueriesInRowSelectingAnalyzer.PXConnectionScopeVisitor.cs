using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public partial class DatabaseQueriesInRowSelectingAnalyzer
	{
		private class PXConnectionScopeVisitor : CSharpSyntaxVisitor<bool>
		{
			private readonly DiagnosticWalker _parent;
			private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;
			private readonly CancellationToken _cancellation;

			public PXConnectionScopeVisitor(DiagnosticWalker parent, PXContext pxContext, SemanticModel semanticModel, 
											CancellationToken cancellation)
			{
				_parent 	   = parent;
				_pxContext 	   = pxContext;
				_semanticModel = semanticModel;
				_cancellation  = cancellation;
			}

			public override bool VisitUsingStatement(UsingStatementSyntax node)
			{
				return (node.Declaration?.Accept(this) ?? false) || (node.Expression?.Accept(this) ?? false);
			}

			public override bool VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				var symbol = _semanticModel.GetSymbolOrFirstCandidate(node.Type, _cancellation);

				return symbol != null && 
					   (symbol.Equals(_pxContext.PXConnectionScope, SymbolEqualityComparer.Default) ||
						symbol.OriginalDefinition?.Equals(_pxContext.PXConnectionScope, SymbolEqualityComparer.Default) == true);
			}
		}
	}
}
