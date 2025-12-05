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
	/// <summary>
	/// An extension type hierarchy collector. Uses depth first search (DFS) based algorithm for cycle detection.
	/// </summary>
	protected sealed class ExtensionTypeHierarchyCollector
	{
		private const int MaxPathLength = 150;

		private readonly SymbolInfoBuilderBase<TRootInfo, TExtensionInfo> _builder;
		private readonly PXContext _pxContext;
		private readonly CancellationToken _cancellation;

		private readonly Dictionary<ITypeSymbol, TExtensionInfo?> _visitedExtensionInfos = new(SymbolEqualityComparer.Default);
		private readonly Stack<ITypeSymbol> _currentPath = new();

		private readonly Dictionary<ITypeSymbol, TRootInfo?> _collectedRootInfos = new(SymbolEqualityComparer.Default);

		public IReadOnlyCollection<ITypeSymbol> CollectedRootTypes => _collectedRootInfos.Keys;

		public IReadOnlyCollection<TRootInfo?> CollectedRootInfos => _collectedRootInfos.Values;

		public ITypeSymbol? CircularReferenceExtension { get; private set; }

		public ITypeSymbol? ExtensionWithBadBaseExtensions { get; private set; }

		public bool FailedToCollectTypeHierarchy { get; private set; }

		public ExtensionTypeHierarchyCollector(SymbolInfoBuilderBase<TRootInfo, TExtensionInfo> builder, PXContext pxContext, 
											   CancellationToken cancellation)
		{
			_builder = builder;
			_pxContext = pxContext;
			_cancellation = cancellation;
		}

		public InferResultKind GetResultKind()
		{
			if (CircularReferenceExtension != null)
				return InferResultKind.CircularReferences;
			else if (_collectedRootInfos.Count > 1)
				return InferResultKind.MultipleRootTypes;
			else if (ExtensionWithBadBaseExtensions != null)
				return InferResultKind.BadBaseExtensions;
			else if (FailedToCollectTypeHierarchy)
				return InferResultKind.UnrecognizedError;
			else
				return InferResultKind.Success;
		}

		public TExtensionInfo? InferExtensionInfo(ITypeSymbol extensionTypeSymbol, int? customDeclarationOrder)
		{
			extensionTypeSymbol.ThrowOnNull();
			_cancellation.ThrowIfCancellationRequested();

			ClearState();
			var extensionInfo = VisitExtensionType(extensionTypeSymbol, isInSource: true, precalcedRootTypeSymbol: null, 
													customDeclarationOrder);
			return extensionInfo;
		}

		private TExtensionInfo? VisitExtensionType(ITypeSymbol extensionTypeSymbol, bool isInSource, 
												   ITypeSymbol? precalcedRootTypeSymbol, int? extensionDeclarationOrder)
		{
			// Stop all infer operations early if the type hierarchy is already recognized as inconsistent 
			if (GetResultKind() != InferResultKind.Success)
				return null;

			if (_visitedExtensionInfos.TryGetValue(extensionTypeSymbol, out var alreadyCalcedInfo))
				return alreadyCalcedInfo;

			if (IsTypeAlreadyVisitedInCurrentPath(extensionTypeSymbol))
			{
				CircularReferenceExtension = extensionTypeSymbol;
				_visitedExtensionInfos[extensionTypeSymbol] = null;			// Cache problem info for extension with proven circular reference
				return null;
			}
			else if (_currentPath.Count > MaxPathLength)
			{
				FailedToCollectTypeHierarchy = true;
				return null;
			}

			int declarationOrder = extensionDeclarationOrder ?? 0;
			_currentPath.Push(extensionTypeSymbol);

			try
			{
				TExtensionInfo? inferredExtensionInfo;
				var extensionNode = isInSource
					? extensionTypeSymbol.GetSyntax(_cancellation) as ClassDeclarationSyntax
					: null;

				// Trivial popular hot path optimization
				if (_builder.DoesExtensionExtendOnlyRootSymbol(extensionTypeSymbol, _pxContext))
				{
					inferredExtensionInfo = InferExtensionExtendingOnlyRootSymbol(extensionTypeSymbol, extensionNode,
																				  precalcedRootTypeSymbol, declarationOrder);
				}
				else
				{
					inferredExtensionInfo = InferExtensionRecursively(extensionTypeSymbol, extensionNode, precalcedRootTypeSymbol, 
																	  declarationOrder);
				}

				_visitedExtensionInfos[extensionTypeSymbol] = inferredExtensionInfo;    // Cache infer failures and successes
				return inferredExtensionInfo;
			}
			finally
			{
				_currentPath.Pop();
			}
		}

		private TExtensionInfo? InferExtensionExtendingOnlyRootSymbol(ITypeSymbol extensionTypeSymbol, ClassDeclarationSyntax? extensionNode,
																	  ITypeSymbol? precalcedRootTypeSymbol, int declarationOrder)
		{
			_cancellation.ThrowIfCancellationRequested();
			var rootTypeSymbol = precalcedRootTypeSymbol ?? _builder.GetRootTypeFromExtensionType(extensionTypeSymbol, _pxContext);

			if (rootTypeSymbol == null)
			{
				FailedToCollectTypeHierarchy = true;
				return null;
			}
			else
			{
				// Use cache of root symbols infos
				if (!_collectedRootInfos.TryGetValue(rootTypeSymbol, out var rootInfo))
				{
					rootInfo = _builder.CreateRootSymbolInfo(rootTypeSymbol, _pxContext, customDeclarationOrder: null, _cancellation);
					_collectedRootInfos[rootTypeSymbol] = rootInfo;
				}

				if (!_builder.CheckBaseExtensionsAreCorrect(Array.Empty<TExtensionInfo>()))
				{
					ExtensionWithBadBaseExtensions = extensionTypeSymbol;
					return null;
				}

				var extensionInfo = _builder.ExtensionSymbolInfoConstructor(extensionNode, extensionTypeSymbol, rootInfo, declarationOrder);
				return extensionInfo;
			}
		}

		private TExtensionInfo? InferExtensionRecursively(ITypeSymbol extensionTypeSymbol, ClassDeclarationSyntax? extensionNode,
														  ITypeSymbol? precalcedRootTypeSymbol, int declarationOrder)
		{
			_cancellation.ThrowIfCancellationRequested();
			INamedTypeSymbol? baseGenericExtensionType = _builder.GetBaseGenericExtensionType(extensionTypeSymbol, _pxContext);

			if (baseGenericExtensionType == null)
			{
				FailedToCollectTypeHierarchy = true;
				return null;
			}

			var rootTypeSymbol = precalcedRootTypeSymbol ?? _builder.GetRootTypeFromExtensionType(extensionTypeSymbol, _pxContext);

			if (rootTypeSymbol == null)
			{
				FailedToCollectTypeHierarchy = true;
				return null;
			}

			// Use cache of root symbols infos
			if (!_collectedRootInfos.TryGetValue(rootTypeSymbol, out var rootInfo))
			{
				rootInfo = _builder.CreateRootSymbolInfo(rootTypeSymbol, _pxContext, customDeclarationOrder: null, _cancellation);
				_collectedRootInfos[rootTypeSymbol] = rootInfo;
			}

			_cancellation.ThrowIfCancellationRequested();
			
			// Extension base type is the base generic extension type, we need to calculate all chained base extensions
			if (IsDerivedFromFromBaseGenericExtensionType(extensionTypeSymbol, baseGenericExtensionType))
			{
				return InferExtensionDerivedFromBaseGenericExtensionType(extensionTypeSymbol, extensionNode, baseGenericExtensionType,
																		 rootTypeSymbol, rootInfo, declarationOrder);
			}
			else	// Extension is derived from some custom extension, we need to get only one base extension info
			{
				return InferExtensionDerivedFromCustomExtension(extensionTypeSymbol, extensionNode, rootTypeSymbol, rootInfo, declarationOrder);
			}
		}

		private bool IsDerivedFromFromBaseGenericExtensionType(ITypeSymbol extensionTypeSymbol, INamedTypeSymbol baseGenericExtensionType)
		{
			if (extensionTypeSymbol is INamedTypeSymbol extensionNamedTypeSymbol)
				return SymbolEqualityComparer.Default.Equals(extensionTypeSymbol.BaseType, baseGenericExtensionType);
			else if (extensionTypeSymbol is ITypeParameterSymbol typeParameterSymbol)
			{
				var constraintTypes = typeParameterSymbol.GetAllConstraintTypes(includeInterfaces: false)
														 .OfType<INamedTypeSymbol>();
				return constraintTypes.Contains(baseGenericExtensionType, SymbolEqualityComparer.Default);
			}	
			else
				return false;
		}

		private TExtensionInfo? InferExtensionDerivedFromCustomExtension(ITypeSymbol extensionTypeSymbol, ClassDeclarationSyntax? extensionNode,
																		 ITypeSymbol rootTypeSymbol, TRootInfo? rootInfo, int declarationOrder)
		{
			if (extensionTypeSymbol.BaseType == null)
			{
				FailedToCollectTypeHierarchy = true;
				return null;
			}

			bool isInSource = extensionNode != null;

			// Small optimization - re-use calculation of root type symbol since it is the same for the base extension type
			var baseExtensionInfo = VisitExtensionType(extensionTypeSymbol.BaseType, isInSource, precalcedRootTypeSymbol: rootTypeSymbol,
													   extensionDeclarationOrder: null);
			if (baseExtensionInfo == null)
				return null;

			if (!_builder.CheckBaseExtensionsAreCorrect([baseExtensionInfo]))
			{
				ExtensionWithBadBaseExtensions = extensionTypeSymbol;
				return null;
			}

			var extensionInfo = _builder.ExtensionSymbolInfoConstructorWithBaseInfo(extensionNode, extensionTypeSymbol, rootInfo,
																					declarationOrder, baseExtensionInfo, 
																					ExtensionMechanismType.Inheritance);
			return extensionInfo;
		}

		private TExtensionInfo? InferExtensionDerivedFromBaseGenericExtensionType(ITypeSymbol extensionTypeSymbol, 
																ClassDeclarationSyntax? extensionNode, INamedTypeSymbol baseGenericExtensionType,
																ITypeSymbol rootTypeSymbol, TRootInfo? rootInfo, int declarationOrder)
		{
			var baseChainedExtensionTypes = GetBaseChainedExtensionTypes(baseGenericExtensionType);

			switch (baseChainedExtensionTypes?.Count)
			{
				case 0:
					return InferExtensionExtendingOnlyRootSymbol(extensionTypeSymbol, extensionNode, rootTypeSymbol, declarationOrder);

				// Optimization - simple and popular case with only one base extension. No compaction of base extension is required in this case
				case 1:
				{
					bool isInSource = extensionNode != null;
					ITypeSymbol baseChainedExtensionType = baseChainedExtensionTypes[0];

					// Deliberately do not use the precalcedRootTypeSymbol here since it can be different for chained extension
					var chainedExtensionInfo = VisitExtensionType(baseChainedExtensionType, isInSource, precalcedRootTypeSymbol: null,
																  extensionDeclarationOrder: null);
					if (chainedExtensionInfo == null)
						return null;

					if (!_builder.CheckBaseExtensionsAreCorrect([chainedExtensionInfo]))
					{
						ExtensionWithBadBaseExtensions = extensionTypeSymbol;
						return null;
					}

					var extensionInfo = _builder.ExtensionSymbolInfoConstructorWithBaseInfo(extensionNode, extensionTypeSymbol, rootInfo,
																							declarationOrder, chainedExtensionInfo,
																							ExtensionMechanismType.Chaining);
					return extensionInfo;
				}
				case null:
					FailedToCollectTypeHierarchy = true;
					return null;
				default:
					return InferExtensionDerivedFromBaseGenericExtensionTypeWithMultipleChainedExtensions(extensionTypeSymbol, extensionNode, rootInfo,
																										  declarationOrder, baseChainedExtensionTypes);
			}
		}

		private IReadOnlyList<ITypeSymbol>? GetBaseChainedExtensionTypes(INamedTypeSymbol baseGenericExtensionType)
		{
			if (!baseGenericExtensionType.IsGenericType)
				return [];

			var typeArguments = baseGenericExtensionType.TypeArguments;

			if (typeArguments.Length <= 1)
				return [];

			var chainedBaseExtensions = _builder.GetChainedBaseExtensionTypesFromBaseGenericExtensionType(baseGenericExtensionType, _pxContext);
			return chainedBaseExtensions;
		}

		private TExtensionInfo? InferExtensionDerivedFromBaseGenericExtensionTypeWithMultipleChainedExtensions(ITypeSymbol extensionTypeSymbol,
																					ClassDeclarationSyntax? extensionNode, TRootInfo? rootInfo, 
																					int declarationOrder, IReadOnlyList<ITypeSymbol> baseChainedExtensionTypes)
		{
			bool isInSource = extensionNode != null;
			var baseChainedExtensionInfos = new List<TExtensionInfo>(baseChainedExtensionTypes.Count);

			foreach (ITypeSymbol chainedExtensionType in baseChainedExtensionTypes)
			{
				// Deliberately do not use the precalcedRootTypeSymbol here since it can be different for chained extensions
				var chainedExtensionInfo = VisitExtensionType(chainedExtensionType, isInSource, precalcedRootTypeSymbol: null,
															  extensionDeclarationOrder: null);
				if (chainedExtensionInfo == null)
					return null;

				baseChainedExtensionInfos.Add(chainedExtensionInfo);
			}

			var compactedBaseChainedExtensionInfos = CompactExtensionInfos(baseChainedExtensionInfos);

			// Check base extensions correctness after they are collected and compacted
			if (!_builder.CheckBaseExtensionsAreCorrect(compactedBaseChainedExtensionInfos))
			{
				ExtensionWithBadBaseExtensions = extensionTypeSymbol;
				return null;
			}

			var extensionInfo = _builder.ExtensionSymbolInfoConstructorWithBaseInfo(extensionNode, extensionTypeSymbol, rootInfo,
																					declarationOrder, compactedBaseChainedExtensionInfos,
																					ExtensionMechanismType.Chaining);
			return extensionInfo;
		}

		/// <summary>
		/// Perform the in-place compaction of the base extension infos list to remove repeated lower-level extension infos<br/>
		/// that are already present in higher-level extension info subtree.
		/// </summary>
		/// <param name="uncompactedExtensionList">List of uncompacted extensions.</param>
		/// <returns>
		/// The compacted list of extension infos.
		/// </returns>
		/// <remarks>
		/// In the current implementation the returned list is the same list as the input one. For performance, this is a mutating operation.<br/>
		/// <br/>
		/// Another important thing is that due to the recursive nature of the infer algorithm the compaction will be also done recursively<br/>
		/// from the most base extension infos to the most derived extensions. Therefore, the final result will be fully compacted extension infos tree.
		/// </remarks>
		private List<TExtensionInfo> CompactExtensionInfos(List<TExtensionInfo> uncompactedExtensionList)
		{
			if (uncompactedExtensionList.Count <= 1)
				return uncompactedExtensionList;

			for (int currentExtensionIndex = 0; currentExtensionIndex < uncompactedExtensionList.Count; currentExtensionIndex++)
			{
				// Here we implicitly rely on the Acumatica Framework rule that higher-level extension come first
				TExtensionInfo currentExtension = uncompactedExtensionList[currentExtensionIndex];
				var baseExtensionsOfCurrentExtension = currentExtension.BaseExtensions;

				if (baseExtensionsOfCurrentExtension.IsDefaultOrEmpty)
					continue;

				var baseExtensionsSubTreeOfCurrentExtension = currentExtension.GetAllBaseExtensionInfosBFS()
																			  .ToList(capacity: baseExtensionsOfCurrentExtension.Length);

				// Check if any of the specified lower-level extensions are already among base types of the current extension.
				// Remove them if they are.
				for (int j = uncompactedExtensionList.Count - 1; j > currentExtensionIndex; j--)
				{
					var lowerOrSameLevelExtension = uncompactedExtensionList[j];
					bool isInBaseExtensionsSubTree =
						baseExtensionsSubTreeOfCurrentExtension.Any(extInfo => lowerOrSameLevelExtension.Symbol.Equals(extInfo.Symbol,
																				SymbolEqualityComparer.Default));
					if (isInBaseExtensionsSubTree)
					{
						uncompactedExtensionList.RemoveAt(j);
					}
				}
			}

			return uncompactedExtensionList;
		}

		private bool IsTypeAlreadyVisitedInCurrentPath(ITypeSymbol typeSymbol) => _currentPath.Count switch
		{
			0 => false,
			1 => SymbolEqualityComparer.Default.Equals(typeSymbol, _currentPath.Peek()),

			// Linear lookup is used here instead of hash table because the expected path length is less than 10,
			// Hash table will have a worse performance than simple linear search in this case
			_ => _currentPath.Contains(typeSymbol, SymbolEqualityComparer.Default)
		};

		private void ClearState()
		{
			if (_visitedExtensionInfos.Count > 0)
				_visitedExtensionInfos.Clear();

			if (_currentPath.Count > 0)
				_currentPath.Clear();

			if (_collectedRootInfos.Count > 0)
				_collectedRootInfos.Clear();

			FailedToCollectTypeHierarchy = false;
			CircularReferenceExtension = null;
			ExtensionWithBadBaseExtensions = null;
		}
	}
}