using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Graph;

public class GraphAndGraphExtInfoBuilder : SymbolInfoBuilderBase<GraphInfo, GraphExtensionInfo>
{
	protected override bool IsRootFrameworkType(ITypeSymbol typeSymbol, PXContext pxContext) =>
		typeSymbol.IsPXGraph(pxContext);
	
	protected override bool IsExtensionType(ITypeSymbol typeSymbol, PXContext pxContext) =>
		typeSymbol.IsPXGraphExtension(pxContext);

	protected override GraphInfo RootSymbolInfoConstructor(ClassDeclarationSyntax? graphNode, ITypeSymbol graphType, int declarationOrder) => 
		new GraphInfo(graphNode, graphType, declarationOrder);

	protected override GraphInfo RootSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? graphNode, ITypeSymbol graphType, 
																		int declarationOrder, GraphInfo baseGraphInfo) => 
		new GraphInfo(graphNode, graphType, declarationOrder, baseGraphInfo);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructor(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																		 GraphInfo? graphInfo, int declarationOrder) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																			GraphInfo? graphInfo, int declarationOrder, GraphExtensionInfo baseExtension,
																			ExtensionMechanismType extensionMechanismType) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder, baseExtension, extensionMechanismType);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																GraphInfo? graphInfo, int declarationOrder, IEnumerable<GraphExtensionInfo> baseExtensions,
																ExtensionMechanismType extensionMechanismType) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder, baseExtensions, extensionMechanismType);

	protected override IEnumerable<ITypeSymbol> GetBaseRootTypesFromDerivedToBase(ITypeSymbol graphTypeSymbol, PXContext pxContext) =>
		graphTypeSymbol.GetGraphBaseTypes();

	protected override ITypeSymbol? GetRootTypeFromExtensionType(ITypeSymbol graphExtension, PXContext pxContext) => 
		graphExtension.GetGraphFromGraphExtension(pxContext);

	protected override bool DoesExtensionExtendOnlyRootSymbol(ITypeSymbol graphExtension, PXContext pxContext) =>
		graphExtension.BaseType.IsGraphExtensionBaseType() &&
		(graphExtension.BaseType.TypeParameters.IsDefault || graphExtension.BaseType.TypeParameters.Length <= 1);

	protected override INamedTypeSymbol? GetBaseGenericExtensionType(ITypeSymbol graphExtension, PXContext pxContext) =>
		graphExtension.GetBaseTypesAndThis()
					  .FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;

	/// <summary>
	/// Gets the chained base graph extension types from the base PXGraphExtension type.
	/// </summary>
	/// <param name="pxGraphExtensionBaseType">The base PXGraphExtension type from which all extensions derive.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The chained base graph extension types.
	/// </returns>
	protected override IReadOnlyList<ITypeSymbol>? GetChainedBaseExtensionTypesFromBaseGenericExtensionType(ITypeSymbol pxGraphExtensionBaseType, 
																											PXContext pxContext) => 
		GraphSymbolHierarchyUtils.GetChainedExtensionTypesFromPxGraphExtensionTypeArgsUnsafe(pxGraphExtensionBaseType, pxContext);
}