using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
	public class StartLongOperationDelegateWalker : DelegatesWalkerBase
	{
		private readonly HashSet<SyntaxNode> _delegates = new HashSet<SyntaxNode>();

		public ImmutableArray<SyntaxNode> Delegates => _delegates.ToImmutableArray();

		public StartLongOperationDelegateWalker(PXContext pxContext, CancellationToken cancellation)
			: base(pxContext, cancellation)
		{
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			IMethodSymbol? methodSymbol = GetSymbol<IMethodSymbol>(node);

			if (methodSymbol == null || node.ArgumentList?.Arguments.Count is null or 0 ||
				!PxContext.AsyncOperations.AllMethodsStartingLongRun.Contains<IMethodSymbol>(methodSymbol, SymbolEqualityComparer.Default))
			{
				base.VisitInvocationExpression(node);
				return;
			}

			var delegateArgument = GetLongRunDelegateArgument(methodSymbol, node);

			if (delegateArgument != null)
			{
				var delegateBody = GetDelegateNode(delegateArgument);

				if (delegateBody != null)
					_delegates.Add(delegateBody);
			}
		}

		private ExpressionSyntax? GetLongRunDelegateArgument(IMethodSymbol methodSymbol, InvocationExpressionSyntax methodNode)
		{
			var arguments = methodNode.ArgumentList.Arguments;

			switch (arguments.Count)
			{
				case 0:
					return null;

				case 1:
					return methodSymbol.IsDeclaredInType(PxContext.AsyncOperations.IGraphLongOperationManager)
						? arguments[0].Expression
						: null;

				default:
				{
					var firstArgument = methodSymbol.Name == DelegateNames.Async.Await
											? arguments[0].Expression
											: arguments[1].Expression;
					return firstArgument;
				}
			}
		}
	}
}