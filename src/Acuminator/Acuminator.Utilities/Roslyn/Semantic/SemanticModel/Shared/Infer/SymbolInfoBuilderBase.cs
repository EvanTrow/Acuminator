using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public abstract partial class SymbolInfoBuilderBase<TRootInfo, TExtensionInfo>
where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaFrameworkTypeInfo
{
	public virtual IInferredAcumaticaFrameworkTypeInfo? InferTypeInfo(ITypeSymbol? typeSymbol, PXContext pxContext,
																	  int? customDeclarationOrder, CancellationToken cancellation)
	{
		pxContext.ThrowOnNull();

		if (typeSymbol?.BaseType == null)
			return null;

		cancellation.ThrowIfCancellationRequested();

		if (IsRootFrameworkType(typeSymbol, pxContext))
		{
			var rootSymbolInfo = CreateRootSymbolInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);
			return rootSymbolInfo;
		}
		else if (IsExtensionType(typeSymbol, pxContext))
		{
			var extensionTypeHierarchyVisitor = GetExtensionTypeHierarchyVisitor(pxContext, cancellation);
			var inferredInfo = extensionTypeHierarchyVisitor.InferExtensionInfo(typeSymbol, customDeclarationOrder);

			if (inferredInfo == null)
				return null;

			return inferredInfo.Value.InferredExtensionInfo as IInferredAcumaticaFrameworkTypeInfo ?? 
				   inferredInfo.Value.ExtensionInfoCandidate;
		}
		else
			return null;
	}

	protected virtual ExtensionTypeHierarchyVisitor GetExtensionTypeHierarchyVisitor(PXContext pxContext, CancellationToken cancellation) =>
		new ExtensionTypeHierarchyVisitor(this, pxContext, cancellation);

	/// <summary>
	/// Determines whether the <paramref name="typeSymbol"/> is a root Acumatica Framework type (for example, graph or DAC).
	/// </summary>
	/// <param name="typeSymbol">The type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// <see langword="true"/> if the <paramref name="typeSymbol"/> is a root Acumatica type; otherwise, <see langword="false"/>.
	/// </returns>
	protected abstract bool IsRootFrameworkType(ITypeSymbol typeSymbol, PXContext pxContext);

	/// <summary>
	/// Determines whether the <paramref name="typeSymbol"/> is an extension type (for example, graph extension or DAC extension).
	/// </summary>
	/// <param name="typeSymbol">The type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// <see langword="true"/> if the <paramref name="typeSymbol"/> is an extension type; otherwise, <see langword="false"/>.
	/// </returns>
	protected abstract bool IsExtensionType(ITypeSymbol typeSymbol, PXContext pxContext);

	/// <summary>
	/// Creates the root symbol information.
	/// </summary>
	/// <param name="rootTypeSymbol">The root type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <param name="customDeclarationOrder">The custom declaration order.</param>
	/// <param name="cancellation">A token that allows processing to be cancelled.</param>
	/// <returns>
	/// The created root symbol information.
	/// </returns>
	protected virtual TRootInfo? CreateRootSymbolInfo(ITypeSymbol rootTypeSymbol, PXContext pxContext,
													  int? customDeclarationOrder, CancellationToken cancellation)
	{
		cancellation.ThrowIfCancellationRequested();

		var rootNode = rootTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;
		var rootBaseTypesFromBaseToDerived = GetBaseRootTypesFromDerivedToBase(rootTypeSymbol).Reverse();
		bool isInSource = rootNode != null;
		TRootInfo? aggregatedBaseRootTypesInfo = null, prevRootInfo = null;

		foreach (ITypeSymbol baseType in rootBaseTypesFromBaseToDerived)
		{
			cancellation.ThrowIfCancellationRequested();

			var baseRootNode = isInSource
				? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
				: null;

			isInSource = baseRootNode != null;
			aggregatedBaseRootTypesInfo = prevRootInfo != null
				? RootSymbolInfoConstructorWithBaseInfo(baseRootNode, baseType, declarationOrder: 0, prevRootInfo)
				: RootSymbolInfoConstructor(baseRootNode, baseType, declarationOrder: 0);

			prevRootInfo = aggregatedBaseRootTypesInfo;
		}

		int rootDeclarationOrder = customDeclarationOrder ?? 0;
		var rootInfo = aggregatedBaseRootTypesInfo != null
			? RootSymbolInfoConstructorWithBaseInfo(rootNode, rootTypeSymbol, rootDeclarationOrder, aggregatedBaseRootTypesInfo)
			: RootSymbolInfoConstructor(rootNode, rootTypeSymbol, rootDeclarationOrder);

		return rootInfo;
	}

	/// <summary>
	/// Gets the base root types of the <paramref name="rootTypeSymbol"/> from the most derived to the most base type.
	/// </summary>
	/// <param name="rootTypeSymbol">The root type symbol.</param>
	/// <returns>
	/// Base root types of the <paramref name="rootTypeSymbol"/>.
	/// </returns>
	protected abstract IEnumerable<ITypeSymbol> GetBaseRootTypesFromDerivedToBase(ITypeSymbol rootTypeSymbol);

	protected abstract TRootInfo RootSymbolInfoConstructor(ClassDeclarationSyntax? node, ITypeSymbol rootType,
														   int declarationOrder);

	protected abstract TRootInfo RootSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? node, ITypeSymbol rootType, 
																	   int declarationOrder, TRootInfo baseRootInfo);

	/// <summary>
	/// Gets the root type from the extension type <paramref name="extensionTypeSymbol"/>.
	/// </summary>
	/// <param name="extensionTypeSymbol">The extension type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The root type from the extension type.
	/// </returns>
	protected abstract ITypeSymbol? GetRootTypeFromExtensionType(ITypeSymbol extensionTypeSymbol, PXContext pxContext);

	/// <summary>
	/// Check if the extension extends only root symbol and no other extension.
	/// </summary>
	/// <param name="extensionTypeSymbol">The extension type symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns/>
	protected abstract bool DoesExtensionExtendOnlyRootSymbol(ITypeSymbol extensionTypeSymbol, PXContext pxContext);

	protected abstract TExtensionInfo ExtensionSymbolInfoConstructor(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol,
																	 TRootInfo? rootInfo, int declarationOrder);

	protected abstract TExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol,
																				 TRootInfo? rootInfo, int declarationOrder, TExtensionInfo baseExtension);

	protected abstract TExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol,
																	TRootInfo? rootInfo, int declarationOrder, IEnumerable<TExtensionInfo> baseExtensions);

	protected abstract ExtensionCandidateInfo<TRootInfo, TExtensionInfo> ExtensionCandidateInfoConstructor(ClassDeclarationSyntax? extensionNode,
																							ITypeSymbol extensionSymbol, int declarationOrder);

	/// <summary>
	/// Gets the base extension types of the <paramref name="extensionTypeSymbol"/> from the most derived to the most base type.
	/// </summary>
	/// <param name="extensionTypeSymbol">The extension type symbol.</param>
	/// <returns>
	/// Base extension types of the <paramref name="extensionTypeSymbol"/>.
	/// </returns>
	protected abstract IEnumerable<ITypeSymbol> GetBaseExtensionTypesFromDerivedToBase(ITypeSymbol extensionTypeSymbol);

	protected abstract INamedTypeSymbol? GetBaseGenericExtensionType(ITypeSymbol extensionTypeSymbol, PXContext pxContext);

	/// <summary>
	/// Gets the chained base extension types from the base generic extension type.
	/// </summary>
	/// <param name="baseGenericExtensionType">The base generic extension type from which all extensions derive.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The chained base extension types.
	/// </returns>
	protected abstract IReadOnlyCollection<ITypeSymbol>? GetChainedBaseExtensionTypesFromBaseGenericExtensionType(ITypeSymbol baseGenericExtensionType,
																												  PXContext pxContext);
}