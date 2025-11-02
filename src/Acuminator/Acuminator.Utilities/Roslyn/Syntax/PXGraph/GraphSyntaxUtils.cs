using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax.PXGraph
{
	public static class GraphSyntaxUtils
	{
		/// <summary>
		/// Determines PXGraph instantiation type for the syntax node (e.g. new PXGraph(), PXGraph.CreateInstance, etc.)
		/// </summary>
		public static GraphInstantiationType GetGraphInstantiationType(this SyntaxNode node, SemanticModel semanticModel, 
			PXContext pxContext)
		{
			node.ThrowOnNull();
			semanticModel.ThrowOnNull();
			pxContext.ThrowOnNull();

			// new PXGraph()
			if (node is ObjectCreationExpressionSyntax objCreationSyntax && objCreationSyntax.Type != null
			                                                             && semanticModel
				                                                             .GetSymbolInfo(objCreationSyntax.Type)
				                                                             .Symbol is ITypeSymbol typeSymbol
			                                                             && typeSymbol.IsPXGraph(pxContext))
			{
				return typeSymbol.Equals(pxContext.PXGraph.Type, SymbolEqualityComparer.Default)
					? GraphInstantiationType.ConstructorOfBaseType
					: GraphInstantiationType.ConstructorOfSpecificType;
			}

			// PXGraph.CreateInstance
			if (node is InvocationExpressionSyntax invocationSyntax)
			{
				var symbolInfo = semanticModel.GetSymbolInfo(invocationSyntax);
				var methodSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
				methodSymbol = methodSymbol?.OverriddenMethod?.OriginalDefinition ?? methodSymbol?.OriginalDefinition;

				if (methodSymbol != null && pxContext.PXGraph.CreateInstance.Contains<IMethodSymbol>(methodSymbol, SymbolEqualityComparer.Default))
				{
					return GraphInstantiationType.CreateInstance;
				}
			}

			return GraphInstantiationType.None;
		}

		internal static IEnumerable<(ITypeSymbol GraphSymbol, ClassDeclarationSyntax GraphNode)> GetDeclaredGraphsAndExtensions(
																				this SyntaxNode root, SemanticModel semanticModel,
																				PXContext context, CancellationToken cancellationToken = default)
		{
			root.ThrowOnNull();
			context.ThrowOnNull();
			semanticModel.ThrowOnNull();
			cancellationToken.ThrowIfCancellationRequested();

			return context.IsPlatformReferenced 
				? GetDeclaredGraphsAndExtensionsImpl()
				: [];

			//------------------------------------------------Local Function------------------------------------------------
			IEnumerable<(ITypeSymbol GraphSymbol, ClassDeclarationSyntax GraphNode)> GetDeclaredGraphsAndExtensionsImpl()
			{
				var declaredClasses = root.DescendantNodesAndSelf()
										  .OfType<ClassDeclarationSyntax>();

				foreach (ClassDeclarationSyntax classDeclaration in declaredClasses)
				{
					var classTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as ITypeSymbol;

					if (classTypeSymbol != null && classTypeSymbol.IsPXGraphOrExtension(context))
					{
						yield return (classTypeSymbol, classDeclaration);
					}
				}
			}
		}
	}
}
