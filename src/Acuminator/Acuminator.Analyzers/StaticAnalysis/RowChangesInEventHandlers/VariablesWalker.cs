using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Collects all variables that are declared inside the method, and assigned with <code>e.Row</code>
		/// </summary>
		private class VariablesWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private CancellationToken _cancellationToken;
			private readonly HashSet<ILocalSymbol>? _variables;
			private readonly EventArgsRowWalker _eventArgsRowWalker;

			private readonly HashSet<ILocalSymbol> _foundRowVariables = new(SymbolEqualityComparer.Default);

			public ImmutableArray<ILocalSymbol> FoundRowVariables => _foundRowVariables.ToImmutableArray();

			public VariablesWalker(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel, PXContext pxContext,
				CancellationToken cancellationToken)
			{
				methodSyntax.ThrowOnNull();

				_semanticModel = semanticModel.CheckIfNull();
				_pxContext = pxContext.CheckIfNull();
				_cancellationToken = cancellationToken;

				_eventArgsRowWalker = new EventArgsRowWalker(semanticModel, pxContext);

				if (methodSyntax.Body != null || methodSyntax.ExpressionBody?.Expression != null)
				{
					var dataFlow = methodSyntax.Body != null
						? semanticModel.AnalyzeDataFlow(methodSyntax.Body)
						: semanticModel.AnalyzeDataFlow(methodSyntax.ExpressionBody!.Expression);

					if (dataFlow?.Succeeded == true)
					{
						_variables = dataFlow.WrittenInside
							.Intersect(dataFlow.VariablesDeclared, SymbolEqualityComparer.Default)
							.OfType<ILocalSymbol>()
							.ToHashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
					}
				}
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignment)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (assignment.Left is IdentifierNameSyntax variableNode && assignment.Right != null)
				{
					var variableSymbol = _semanticModel.GetSymbolInfo(variableNode, _cancellationToken).Symbol as ILocalSymbol;
					ValidateThatVariableIsSetToDacFromEvent(variableSymbol, assignment.Right);
				}
			}

			public override void VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				foreach (var variableDeclarator in variableDeclaration.Variables.Where(v => v.Initializer?.Value != null))
				{
					var variableSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator, _cancellationToken) as ILocalSymbol;
					ValidateThatVariableIsSetToDacFromEvent(variableSymbol, variableDeclarator.Initializer!.Value);
				}
			}

			public override void VisitIsPatternExpression(IsPatternExpressionSyntax isPatternExpression)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (_variables == null)
					return;

				_eventArgsRowWalker.Reset();
				isPatternExpression.Expression.Accept(_eventArgsRowWalker);

				if (_eventArgsRowWalker.FoundRowProperty == null)
					return;

				IPropertySymbol rowProperty = _eventArgsRowWalker.FoundRowProperty;
				var variableDesignations	= isPatternExpression.Pattern.GetAllVariableDesignations();

				foreach (SingleVariableDesignationSyntax variableDesignation in variableDesignations)
				{
					_cancellationToken.ThrowIfCancellationRequested();

					var variableSymbol = _semanticModel.GetDeclaredSymbol(variableDesignation, _cancellationToken) as ILocalSymbol;

					if (variableSymbol?.Type == null || !_variables.Contains(variableSymbol) ||
						!variableSymbol.Type.Equals(rowProperty.Type, SymbolEqualityComparer.Default))  // Filter out variables with types different from the found property type
					{
						continue;
					}

					_foundRowVariables.Add(variableSymbol);
				}
			}

			private void ValidateThatVariableIsSetToDacFromEvent(ILocalSymbol? variableSymbol, ExpressionSyntax variableInitializerExpression)
			{
				if (variableSymbol == null || _variables == null || !_variables.Contains(variableSymbol))
					return;

				_eventArgsRowWalker.Reset();
				variableInitializerExpression.Accept(_eventArgsRowWalker);

				if (_eventArgsRowWalker.Success)
					_foundRowVariables.Add(variableSymbol);
			}
		}
	}
}
