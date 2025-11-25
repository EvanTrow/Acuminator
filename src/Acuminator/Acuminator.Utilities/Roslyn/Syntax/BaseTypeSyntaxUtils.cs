using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Utilities.Roslyn.Syntax;

/// <summary>
/// Utilities for the retrieval of the information about base type nodes.
/// </summary>
public static class BaseTypeSyntaxUtils
{
	private enum BaseTypeSearchMode : byte
	{
		Graph,
		GraphExtension,
		Dac,
		DacExtension
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (ITypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseDacTypeInfo(SemanticModel semanticModel,
																					PXContext pxContext, ClassDeclarationSyntax? dacNode,
																					CancellationToken cancellation) =>
		GetBaseTypeInfo(semanticModel, pxContext, dacNode, BaseTypeSearchMode.Dac, cancellation);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (ITypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseDacExtensionTypeInfo(SemanticModel semanticModel,
																					PXContext pxContext, ClassDeclarationSyntax? dacExtensionNode,
																					CancellationToken cancellation) =>
		GetBaseTypeInfo(semanticModel, pxContext, dacExtensionNode, BaseTypeSearchMode.DacExtension, cancellation);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (ITypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseGraphTypeInfo(SemanticModel semanticModel,
																					PXContext pxContext, ClassDeclarationSyntax? graphNode,
																					CancellationToken cancellation) =>
		GetBaseTypeInfo(semanticModel, pxContext, graphNode, BaseTypeSearchMode.Graph, cancellation);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (ITypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseGraphExtensionTypeInfo(SemanticModel semanticModel,
																					PXContext pxContext, ClassDeclarationSyntax? graphExtensionNode,
																					CancellationToken cancellation) =>
		GetBaseTypeInfo(semanticModel, pxContext, graphExtensionNode, BaseTypeSearchMode.GraphExtension, cancellation);

	private static (ITypeSymbol TypeSymbol, TypeSyntax TypeNode)? GetBaseTypeInfo(SemanticModel semanticModel, PXContext pxContext, 
																			ClassDeclarationSyntax? typeNode, BaseTypeSearchMode baseTypeSearchMode,
																			CancellationToken cancellation)
	{
		semanticModel.ThrowOnNull();
		pxContext.ThrowOnNull();

		if (typeNode?.BaseList == null)
			return null;

		var baseTypes = typeNode.BaseList.Types;

		if (baseTypes.Count == 0)
			return null;

		foreach (var baseTypeNode in baseTypes)
		{
			cancellation.ThrowIfCancellationRequested();

			if (baseTypeNode?.Type == null ||
				semanticModel.GetTypeInfo(baseTypeNode.Type).Type is not { } baseTypeSymbol)
			{
				continue;
			}

			if (IsBaseTypeToReturn(baseTypeSymbol))
				return (baseTypeSymbol, baseTypeNode.Type);
		}

		return null;

		//----------------------------------------------Local Function-----------------------------------------------
		bool IsBaseTypeToReturn(ITypeSymbol baseTypeSymbol) => baseTypeSearchMode switch
		{
			BaseTypeSearchMode.Graph 		  => baseTypeSymbol.IsPXGraph(pxContext),
			BaseTypeSearchMode.GraphExtension => baseTypeSymbol.IsPXGraphExtension(pxContext),
			BaseTypeSearchMode.Dac 			  => baseTypeSymbol.IsDAC(pxContext),
			BaseTypeSearchMode.DacExtension   => baseTypeSymbol.IsDacExtension(pxContext),
			_ 								  => false,
		};
	}
}
