using System;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public abstract class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IOverridableItem<GraphOrGraphExtInfoBase>
	{
		public GraphOrGraphExtInfoBase? Base { get; }

		protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol graphOrGraphExt, int declarationOrder, GraphOrGraphExtInfoBase baseInfo) :
									 this(node, graphOrGraphExt, declarationOrder)
		{
			Base = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol graphOrGraphExt, int declarationOrder) :
									 base(node, graphOrGraphExt, declarationOrder)
		{
		}

		/// <inheritdoc cref="IWriteableBaseItem{T}.CombineWithBaseInfo(T)"/>
		protected abstract void CombineWithBaseInfo(GraphOrGraphExtInfoBase baseInfo);
	}
}