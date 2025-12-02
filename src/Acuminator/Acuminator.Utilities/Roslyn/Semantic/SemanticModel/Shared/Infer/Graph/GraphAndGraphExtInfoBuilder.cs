using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

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
	
	protected override ExtensionCandidateInfo<GraphInfo, GraphExtensionInfo> ExtensionCandidateInfoConstructor(ClassDeclarationSyntax? graphExtensionNode, 
																							ITypeSymbol graphExtension, int declarationOrder) =>
		new GraphExtensionCandidateInfo(graphExtensionNode, graphExtension, declarationOrder);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructor(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																		 GraphInfo? graphInfo, int declarationOrder) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																			GraphInfo? graphInfo, int declarationOrder, GraphExtensionInfo baseExtension) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder, baseExtension);

	protected override GraphExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol graphExtension,
																GraphInfo? graphInfo, int declarationOrder, IEnumerable<GraphExtensionInfo> baseExtensions) => 
		new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, declarationOrder, baseExtensions);

	protected override IEnumerable<ITypeSymbol> GetBaseExtensionTypesFromDerivedToBase(ITypeSymbol graphExtension) =>
		 graphExtension.GetGraphExtensionBaseTypes();

	protected override IEnumerable<ITypeSymbol> GetBaseRootTypesFromDerivedToBase(ITypeSymbol graphTypeSymbol) =>
		graphTypeSymbol.GetGraphBaseTypes();

	protected override ITypeSymbol? GetRootTypeFromExtensionType(ITypeSymbol graphExtension, PXContext pxContext) => 
		graphExtension.GetGraphFromGraphExtension(pxContext);

	protected override bool DoesExtensionExtendOnlyRootSymbol(ITypeSymbol graphExtension, PXContext pxContext) =>
		graphExtension.BaseType.IsGraphExtensionBaseType() && graphExtension.BaseType.TypeParameters.Length == 1;

	protected override INamedTypeSymbol? GetExtensionBaseType(ITypeSymbol graphExtension, PXContext pxContext) =>
		graphExtension.GetBaseTypesAndThis()
					  .FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
}