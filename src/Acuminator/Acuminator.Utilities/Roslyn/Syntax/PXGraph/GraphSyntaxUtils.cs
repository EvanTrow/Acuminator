using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

		internal static IEnumerable<(ITypeSymbol GraphSymbol, SyntaxNode GraphNode)> GetDeclaredGraphsAndExtensions(
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


			IEnumerable<(ITypeSymbol GraphSymbol, SyntaxNode GraphNode)> GetDeclaredGraphsAndExtensionsImpl()
			{
				var declaredClasses = root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();

				foreach (ClassDeclarationSyntax classNode in declaredClasses)
				{
					ITypeSymbol? classTypeSymbol = classNode.GetTypeSymbolFromClassDeclaration(semanticModel, cancellationToken);

					if (classTypeSymbol != null && classTypeSymbol.IsPXGraphOrExtension(context))
					{
						yield return (classTypeSymbol, classNode);
					}
				}
			}
		}

		public static ITypeSymbol? GetTypeSymbolFromClassDeclaration(this ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel,
																	 CancellationToken cancellationToken = default)
		{
			classDeclaration.ThrowOnNull();
			semanticModel.ThrowOnNull();
			cancellationToken.ThrowIfCancellationRequested();

			var typeSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as ITypeSymbol;

			if (typeSymbol != null)
				return typeSymbol;

			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(classDeclaration, cancellationToken);
			typeSymbol = symbolInfo.Symbol as ITypeSymbol;

			if (typeSymbol == null && symbolInfo.CandidateSymbols.Length == 1)
			{
				typeSymbol = symbolInfo.CandidateSymbols[0] as ITypeSymbol;
			}

			return typeSymbol;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (INamedTypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseGraphTypeInfo(SemanticModel semanticModel, 
																								PXContext pxContext, ClassDeclarationSyntax? graphNode,
																								CancellationToken cancellation) =>
			GetBaseGraphOrExtensionTypeInfo(semanticModel, pxContext, graphNode, lookForGraphExtension: false, cancellation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (INamedTypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseGraphExtensionTypeInfo(SemanticModel semanticModel,
																										PXContext pxContext, ClassDeclarationSyntax? graphNode,
																										CancellationToken cancellation) =>
			GetBaseGraphOrExtensionTypeInfo(semanticModel, pxContext, graphNode, lookForGraphExtension: true, cancellation);

		private static (INamedTypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseGraphOrExtensionTypeInfo(SemanticModel semanticModel,
																						PXContext pxContext, ClassDeclarationSyntax? graphNode,
																						bool lookForGraphExtension,  CancellationToken cancellation)
		{
			semanticModel.ThrowOnNull();
			pxContext.ThrowOnNull();

			if (graphNode?.BaseList == null)
				return null;

			var baseTypes = graphNode.BaseList.Types;

			if (baseTypes.Count == 0)
				return null;

			foreach (var typeSyntax in baseTypes)
			{
				cancellation.ThrowIfCancellationRequested();

				if (typeSyntax?.Type == null ||
					semanticModel.GetTypeInfo(typeSyntax.Type).Type is not INamedTypeSymbol baseTypeSymbol)
				{
					continue;
				}

				if (lookForGraphExtension)
				{
					if (baseTypeSymbol.IsPXGraphExtension(pxContext))
						return (baseTypeSymbol, typeSyntax.Type);
				}
				else if (baseTypeSymbol.IsPXGraph(pxContext))
					return (baseTypeSymbol, typeSyntax.Type);
			}

			return null;
		}
	}
}
