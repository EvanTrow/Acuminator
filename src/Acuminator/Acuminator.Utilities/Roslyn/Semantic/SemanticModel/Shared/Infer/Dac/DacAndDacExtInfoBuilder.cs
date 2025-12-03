using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Dac;

public partial class DacAndDacExtInfoBuilder : SymbolInfoBuilderBase<DacInfo, DacExtensionInfo>
{
	protected override ExtensionTypeHierarchyVisitor GetExtensionTypeHierarchyVisitor(PXContext pxContext, CancellationToken cancellation) => 
		new DacExtensionTypeHierarchyVisitor(this, pxContext, cancellation);

	protected override bool IsRootFrameworkType(ITypeSymbol typeSymbol, PXContext pxContext) =>
		typeSymbol.IsDAC(pxContext);
	
	protected override bool IsExtensionType(ITypeSymbol typeSymbol, PXContext pxContext) =>
		typeSymbol.IsDacExtension(pxContext);

	protected override DacInfo RootSymbolInfoConstructor(ClassDeclarationSyntax? dacNode, ITypeSymbol dacType, int declarationOrder) => 
		new DacInfo(dacNode, dacType, declarationOrder);

	protected override DacInfo RootSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? dacNode, ITypeSymbol dacType, 
																	 int declarationOrder, DacInfo baseDacInfo) => 
		new DacInfo(dacNode, dacType, declarationOrder, baseDacInfo);

	protected override DacExtensionInfo ExtensionSymbolInfoConstructor(ClassDeclarationSyntax? dacExtensionNode, ITypeSymbol dacExtension,
																	   DacInfo? dacInfo, int declarationOrder) => 
		new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, declarationOrder);

	protected override DacExtensionInfo ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? dacExtensionNode, ITypeSymbol dacExtension,
																		DacInfo? dacInfo, int declarationOrder, DacExtensionInfo baseExtension) => 
		new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, declarationOrder, baseExtension);

	protected override DacExtensionInfo? ExtensionSymbolInfoConstructorWithBaseInfo(ClassDeclarationSyntax? dacExtensionNode, ITypeSymbol dacExtension,
																					DacInfo? dacInfo, int declarationOrder,
																					IEnumerable<DacExtensionInfo> baseExtensions) =>
		null;

	protected override IEnumerable<ITypeSymbol> GetBaseRootTypesFromDerivedToBase(ITypeSymbol dacTypeSymbol, PXContext pxContext) =>
		dacTypeSymbol.GetDacBaseTypesThatMayStoreDacProperties(pxContext);

	protected override ITypeSymbol? GetRootTypeFromExtensionType(ITypeSymbol dacExtension, PXContext pxContext) =>
		dacExtension.GetDacFromDacExtension(pxContext);

	protected override bool DoesExtensionExtendOnlyRootSymbol(ITypeSymbol dacExtension, PXContext pxContext) =>
		dacExtension.BaseType.IsDacExtensionBaseType() && 
		(dacExtension.BaseType.TypeParameters.IsDefault || dacExtension.BaseType.TypeParameters.Length <= 1);

	protected override INamedTypeSymbol? GetBaseGenericExtensionType(ITypeSymbol dacExtension, PXContext pxContext) =>
		dacExtension.GetBaseTypesAndThis()
					.FirstOrDefault(type => type.IsDacExtensionBaseType()) as INamedTypeSymbol;

	/// <summary>
	/// Gets the chained base DAC extension types from the base PXCacheExtension type.
	/// </summary>
	/// <param name="pxDacExtensionBaseType">The base PXCacheExtension type from which all extensions derive.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <returns>
	/// The chained base DAC extension types.
	/// </returns>
	protected override IReadOnlyCollection<ITypeSymbol>? GetChainedBaseExtensionTypesFromBaseGenericExtensionType(ITypeSymbol pxDacExtensionBaseType,
																												  PXContext pxContext) => 
		DacSymbolsHierarchyUtils.GetChainedExtensionTypesFromPxCacheExtensionTypeArgsUnsafe(pxDacExtensionBaseType, pxContext);
}