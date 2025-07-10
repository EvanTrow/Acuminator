#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Utilities.Roslyn.Walkers
{
	/// <summary>
	/// A recursive walker that reports graph creation.
	/// </summary>
	public class PXGraphCreateInstanceWalker : NestedInvocationWalker
	{
		private readonly SymbolAnalysisContext _context;
		private readonly DiagnosticDescriptor _descriptor;

		public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
			: base(pxContext, context.CancellationToken)
		{
			_context = context;
			_descriptor = descriptor.CheckIfNull();
		}

		public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			IMethodSymbol? symbol = GetSymbol<IMethodSymbol>(node);

			if (symbol != null && PxContext.PXGraph.CreateInstance.Contains<IMethodSymbol>(symbol.ConstructedFrom, SymbolEqualityComparer.Default))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
			}
			else
			{
				base.VisitMemberAccessExpression(node);
			}
		}

		/// <summary>
		/// Called when the visitor visits a <see cref="ObjectCreationExpressionSyntax"/> node which represents a constructor call via "<c><see langword="new"/> MyGraph()</c>".<br/>
		/// </summary>
		/// <param name="node">The node.</param>
		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			ITypeSymbol? createdObjectType = GetSymbol<ITypeSymbol>(node.Type);

			if (createdObjectType != null && createdObjectType.IsPXGraph(PxContext))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
			}
			else
			{
				base.VisitObjectCreationExpression(node);
			}
		}

		/// <summary>
		/// Called when the visitor visits a <see cref="ImplicitObjectCreationExpressionSyntax"/> node which represents a constructor call via "<c><see langword="new"/>()</c>".<br/>
		/// </summary>
		/// <param name="node">The node.</param>
		public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			var constructor = GetSymbol<IMethodSymbol>(node);

			if (constructor == null || constructor.MethodKind != MethodKind.Constructor)
			{
				base.VisitImplicitObjectCreationExpression(node);
				return;
			}

			if (constructor?.ContainingType != null && constructor.ContainingType.IsPXGraph(PxContext))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
			}
			else
			{
				base.VisitImplicitObjectCreationExpression(node);
			}
		}
	}
}
