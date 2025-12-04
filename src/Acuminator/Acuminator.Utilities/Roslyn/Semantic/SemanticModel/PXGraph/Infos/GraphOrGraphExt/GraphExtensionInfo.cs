using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the correctly declared graph extension without issues in the type hierarchy.
	/// </summary>
	public class GraphExtensionInfo : GraphOrGraphExtInfoBase
	{
		public GraphInfo? BaseGraph { get; }

		/// <summary>
		/// The overridden base graph extensions if any.<br/>
		/// Contains either direct base graph extension or chained graph extensions.
		/// </summary>
		ImmutableArray<GraphExtensionInfo> BaseGraphExtensions { get; }

		public bool IsFirstLevelExtension => BaseGraphExtensions.IsDefaultOrEmpty;

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder) :
							 this(node, graphExtension, graph, declarationOrder, baseGraphExtensions: [])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, GraphExtensionInfo baseGraphExtension) :
							 this(node, graphExtension, graph, declarationOrder,
								  [baseGraphExtension.CheckIfNull()])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, IEnumerable<GraphExtensionInfo> baseGraphExtensions) :
							 this(node, graphExtension, graph, declarationOrder, 
								  baseGraphExtensions?.ToImmutableArray() ?? [])
		{ }

		internal GraphExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol graphExtension, GraphInfo? graph,
									int declarationOrder, ImmutableArray<GraphExtensionInfo> baseGraphExtensions) :
							 base(node, graphExtension, declarationOrder)
		{
			BaseGraph = graph;
			BaseGraphExtensions = baseGraphExtensions;
			CombineWithBaseGraphAndGraphExtensions();
		}

		/// <summary>
		/// Combine this info with info from base graph and graph extensions.
		/// </summary>
		private void CombineWithBaseGraphAndGraphExtensions()
		{

		}
	}
}