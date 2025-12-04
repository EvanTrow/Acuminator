using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

public abstract partial class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, ITypeSymbol>, IInferredAcumaticaSymbolInfo

{
	public abstract ITypeSymbol? GraphType { get; }

	protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol graphOrGraphExt, int declarationOrder) :
								 base(node, graphOrGraphExt, declarationOrder)
	{ }

	public abstract IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromDerivedExtensionToBaseGraph(bool includeSelf);

	public abstract IEnumerable<GraphOrGraphExtInfoBase> GetInfosFromBaseGraphToDerivedExtension(bool includeSelf);

	private IEnumerable<TRawData> GetRawData<TRawData>(bool includeFromBaseInfos,
													   Func<GraphOrGraphExtInfoBase, IEnumerable<TRawData>> rawDataGetter)
	{
		if (includeFromBaseInfos)
		{
			return GetInfosFromBaseGraphToDerivedExtension(includeFromBaseInfos)
					  .SelectMany(graphOrGraphExtInfo => rawDataGetter(graphOrGraphExtInfo));
		}
		else
		{
			return rawDataGetter(this);
		}
	}
}