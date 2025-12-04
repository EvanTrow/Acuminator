using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

public abstract partial class SymbolInfoBuilderBase<TRootInfo, TExtensionInfo>
where TRootInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo
where TExtensionInfo : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IExtensionInfo<TExtensionInfo>, IInferredAcumaticaSymbolInfo
{
	public virtual InferredSymbolInfo? InferTypeInfo(ITypeSymbol? typeSymbol, PXContext pxContext,
													 int? customDeclarationOrder, CancellationToken cancellation)
	{
		pxContext.ThrowOnNull();

		if (typeSymbol?.BaseType == null)
			return null;

		cancellation.ThrowIfCancellationRequested();

		if (IsRootFrameworkType(typeSymbol, pxContext))
		{
			var rootSymbolInfo = CreateRootSymbolInfo(typeSymbol, pxContext, customDeclarationOrder, cancellation);
			return rootSymbolInfo != null
				? new InferredSymbolInfo(rootSymbolInfo, collectedRootTypes: [typeSymbol])
				: null;
		}
		else if (IsExtensionType(typeSymbol, pxContext))
		{
			var extensionTypeHierarchyVisitor = GetExtensionTypeHierarchyVisitor(pxContext, cancellation);
			var inferredExtensionInfo = extensionTypeHierarchyVisitor.InferExtensionInfo(typeSymbol, customDeclarationOrder);

			if (inferredExtensionInfo == null)
				return null;

			return new InferredSymbolInfo(inferredExtensionInfo, extensionTypeHierarchyVisitor.CollectedRootTypes)
			{
				CircularReferenceExtension	   = extensionTypeHierarchyVisitor.CircularReferenceExtension,
				ExtensionWithBadBaseExtensions = extensionTypeHierarchyVisitor.ExtensionWithBadBaseExtensions,
				FailedToCollectTypeHierarchy   = extensionTypeHierarchyVisitor.FailedToCollectTypeHierarchy
			};
		}
		else
			return null;
	}

	protected ExtensionTypeHierarchyVisitor GetExtensionTypeHierarchyVisitor(PXContext pxContext, CancellationToken cancellation) =>
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
		var rootBaseTypesFromBaseToDerived = GetBaseRootTypesFromDerivedToBase(rootTypeSymbol, pxContext).Reverse();
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
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// Base root types of the <paramref name="rootTypeSymbol"/>.
	/// </returns>
	protected abstract IEnumerable<ITypeSymbol> GetBaseRootTypesFromDerivedToBase(ITypeSymbol rootTypeSymbol, PXContext pxContext);

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
																				 TRootInfo? rootInfo, int declarationOrder, TExtensionInfo baseExtension,
																				 ExtensionMechanismType extensionMechanismType);

	protected abstract TExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? extensionNode, ITypeSymbol extensionSymbol,
																TRootInfo? rootInfo, int declarationOrder, IReadOnlyCollection<TExtensionInfo> baseExtensions, 
																ExtensionMechanismType extensionMechanismType);

	protected abstract INamedTypeSymbol? GetBaseGenericExtensionType(ITypeSymbol extensionTypeSymbol, PXContext pxContext);

	/// <summary>
	/// Gets the chained base extension types from the base generic extension type.
	/// </summary>
	/// <param name="baseGenericExtensionType">The base generic extension type from which all extensions derive.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The chained base extension types.
	/// </returns>
	protected abstract IReadOnlyList<ITypeSymbol>? GetChainedBaseExtensionTypesFromBaseGenericExtensionType(ITypeSymbol baseGenericExtensionType,
																											PXContext pxContext);

	/// <summary>
	/// Check that the inferred base extensions are correct.
	/// </summary>
	/// <param name="baseExtensions">The base extensions.</param>
	/// <returns>
	/// True if base extensions are correct, false if not.
	/// </returns>
	protected abstract bool CheckBaseExtensionsAreCorrect(IReadOnlyCollection<TExtensionInfo> baseExtensions);
}