using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax.Trivia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for code generation.
	/// </summary>
	public static class CodeGeneration
	{
		/// <summary>
		/// Adds a missing using directive for namespace with name <paramref name="namespaceName"/> at the end of the using directives list.
		/// If the namespace is present does not return anything.
		/// </summary>
		/// <param name="root">The root node.</param>
		/// <param name="namespaceName">Name of the namespace.</param>
		/// <param name="insertSystemNamespaceFirst">True to insert <see cref="System"/> namespace first.</param>
		/// <returns>
		/// The root node with added using directive.
		/// </returns>
		public static CompilationUnitSyntax AddMissingUsingDirectiveForNamespace(this CompilationUnitSyntax root, string namespaceName, 
																				 bool insertSystemNamespaceFirst = true)
		{
			root.ThrowOnNull();
			namespaceName.ThrowOnNullOrWhiteSpace();

			var usings = root.Usings;
			bool alreadyHasUsing = usings.Any(usingDirective => namespaceName == usingDirective.Name?.ToString());

			if (alreadyHasUsing)
				return root;

			bool isSystemNamespace = namespaceName == NamespaceNames.System ||
									 namespaceName.StartsWith(NamespaceNames.SystemWithDot, StringComparison.Ordinal);
			var newUsingDirective = UsingDirective(
										ParseName(namespaceName));

			if (isSystemNamespace && insertSystemNamespaceFirst)
			{
				(newUsingDirective, var newOldFirstUsingDirective) = MoveCompilerDirectivesFromPreviousFirstUsing(newUsingDirective);
				SyntaxList<UsingDirectiveSyntax> newUsings;

				if (newOldFirstUsingDirective != null)
				{
					newUsings = usings.RemoveAt(0)
									  .InsertRange(0, [newUsingDirective, newOldFirstUsingDirective]);
				}
				else
				{
					// Old first using node was not modified - no compiler directives were moved from it
					// So, no need to update its node
					newUsings = usings.Insert(0, newUsingDirective);
				}

				return root.WithUsings(newUsings);
			}
			else
				return root.AddUsings(newUsingDirective);

			//-----------------------------------------------Local Function--------------------------------------------------
			(UsingDirectiveSyntax NewFirstUsing, UsingDirectiveSyntax? NewOldFirstUsingDirective) MoveCompilerDirectivesFromPreviousFirstUsing(
																									UsingDirectiveSyntax usingDirectiveToCopyTriviaTo)
			{
				if (usings.Count == 0)
					return (usingDirectiveToCopyTriviaTo, NewOldFirstUsingDirective: null);

				var previousFirstUsing = usings[0];
				var previousFirstUsingLeadingTrivia = previousFirstUsing.GetLeadingTrivia();

				if (previousFirstUsingLeadingTrivia.Count == 0 || !previousFirstUsingLeadingTrivia.Any(trivia => trivia.IsDirective))
					return (usingDirectiveToCopyTriviaTo, NewOldFirstUsingDirective: null);

				var usingDirectiveWithTrivia = CopyCompilerDirectivesFromTrivia(usingDirectiveToCopyTriviaTo, previousFirstUsingLeadingTrivia, 
																				copyBeforeNode: true, insertCopiedTriviaAfterNodeTrivia: false);
				var previousFirstUsingLeadingTriviaWithoutDirectives = RemoveCompilerDirectivesFromTrivia(previousFirstUsingLeadingTrivia);

				var newPreviousFirstUsing = previousFirstUsingLeadingTriviaWithoutDirectives != null
					? previousFirstUsing.WithLeadingTrivia(previousFirstUsingLeadingTriviaWithoutDirectives)
					: previousFirstUsing;

				return (usingDirectiveWithTrivia, newPreviousFirstUsing);
			}
		}

		/// <summary>
		/// Create attribute list of the supplied type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static AttributeListSyntax GetAttributeList(this INamedTypeSymbol type, AttributeArgumentListSyntax? argumentList = null)
		{
			type.ThrowOnNull();

			var node = Attribute(
						IdentifierName(
							type.Name))
						.WithAdditionalAnnotations(Simplifier.Annotation);

			if (argumentList != null)
			{
				node = node.WithArgumentList(argumentList);
			}

			var list = AttributeList(
						SingletonSeparatedList(
							node));

			return list;
		}

		public static TNode CopyCompilerDirectivesFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithDirectives,
																	bool copyBeforeNode, bool insertCopiedTriviaAfterNodeTrivia)
		where TNode : SyntaxNode
		{
			return CopyDirectivesFromTrivia(nodeToCopyTrivia, triviaWithDirectives, copyBeforeNode, insertCopiedTriviaAfterNodeTrivia,
											regionKindsToCopy: null);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TNode CopyRegionsFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithRegions,
														 bool copyBeforeNode, bool insertCopiedRegionsAfterNodeTrivia,
														 RegionDirectiveSearchMode regionKindsToCopy)
		where TNode : SyntaxNode
		{
			return CopyDirectivesFromTrivia(nodeToCopyTrivia, triviaWithRegions, copyBeforeNode, insertCopiedRegionsAfterNodeTrivia,
											regionKindsToCopy);
		}

		private static TNode CopyDirectivesFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithDirectives,
														 bool copyBeforeNode, bool insertCopiedTriviaAfterNodeTrivia,
														 RegionDirectiveSearchMode? regionKindsToCopy)
		where TNode : SyntaxNode
		{
			nodeToCopyTrivia.ThrowOnNull();

			if (regionKindsToCopy == RegionDirectiveSearchMode.None)
				return nodeToCopyTrivia;

			var directiveTrivias = regionKindsToCopy.HasValue
				? triviaWithDirectives.GetRegionDirectiveLines(regionKindsToCopy.Value)
				: triviaWithDirectives.GetCompilerDirectives();

			if (directiveTrivias.Count == 0)
				return nodeToCopyTrivia;

			if (copyBeforeNode)
			{
				var nodeLeadingTrivia = nodeToCopyTrivia.GetLeadingTrivia();
				var newNodeLeadingTrivia = insertCopiedTriviaAfterNodeTrivia
					? nodeLeadingTrivia.AddRange(directiveTrivias)
					: nodeLeadingTrivia.InsertRange(0, directiveTrivias);

				return nodeToCopyTrivia.WithLeadingTrivia(newNodeLeadingTrivia);
			}
			else
			{
				var nodeTrailingTrivia = nodeToCopyTrivia.GetTrailingTrivia();
				var newNodeTrailingTrivia = insertCopiedTriviaAfterNodeTrivia
					? nodeTrailingTrivia.AddRange(directiveTrivias)
					: nodeTrailingTrivia.InsertRange(0, directiveTrivias);

				return nodeToCopyTrivia.WithTrailingTrivia(newNodeTrailingTrivia);
			}
		}

		/// <summary>
		/// Removes the regions from the <paramref name="node"/>'s leading trivia.
		/// </summary>
		/// <typeparam name="TNode">Type of the node.</typeparam>
		/// <param name="node">The syntax node to act on.</param>
		/// <param name="regionKindsToRemove">The region kinds to remove.</param>
		/// <returns>
		/// The node with removed regions from leading trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(node))]
		public static TNode? RemoveRegionsFromLeadingTrivia<TNode>(this TNode? node, RegionDirectiveSearchMode regionKindsToRemove)
		where TNode : SyntaxNode
		{
			if (node == null)
				return node;

			var leadingTrivia	 = node.GetLeadingTrivia();
			var newLeadingTrivia = RemoveRegionsFromTrivia(leadingTrivia, regionKindsToRemove);

			return newLeadingTrivia != null
				? node.WithLeadingTrivia(newLeadingTrivia)
				: node;
		}

		/// <summary>
		/// Removes the regions from the <paramref name="node"/>'s trailing trivia.
		/// </summary>
		/// <typeparam name="TNode">Type of the node.</typeparam>
		/// <param name="node">The syntax node to act on.</param>
		/// <param name="regionKindsToRemove">The region kinds to remove.</param>
		/// <returns>
		/// The node with removed regions from trailing trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(node))]
		public static TNode? RemoveRegionsFromTrailingTrivia<TNode>(this TNode? node, RegionDirectiveSearchMode regionKindsToRemove)
		where TNode : SyntaxNode
		{
			if (node == null)
				return node;

			var trailingTrivia	  = node.GetTrailingTrivia();
			var newTrailingTrivia = RemoveRegionsFromTrivia(trailingTrivia, regionKindsToRemove);

			return newTrailingTrivia != null
				? node.WithTrailingTrivia(newTrailingTrivia)
				: node;
		}

		/// <summary>
		/// Removes the regions from the <paramref name="trivia"/> collection.
		/// </summary>
		/// <param name="trivia">The trivia.</param>
		/// <param name="regionKindsToRemove">The region kinds to remove.</param>
		/// <returns>
		/// If there was some trivia removed then returns new trivia collection without regions.<br/>
		/// Otherwise, if there was no trivias to remove, returns <see langword="null"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia>? RemoveRegionsFromTrivia(in SyntaxTriviaList trivia, 
																		 RegionDirectiveSearchMode regionKindsToRemove) =>
			RemoveDirectivesFromTrivia(trivia, regionKindsToRemove);

		/// <summary>
		/// Removes the compiler directives from the <paramref name="trivia"/> collection.
		/// </summary>
		/// <param name="trivia">The trivia.</param>
		/// <returns>
		/// If there was some trivia removed then returns new trivia collection without regions.<br/>
		/// Otherwise, if there was no trivias to remove, returns <see langword="null"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia>? RemoveCompilerDirectivesFromTrivia(in SyntaxTriviaList trivia) =>
			RemoveDirectivesFromTrivia(trivia, regionKindsToRemove: null);

		private static IEnumerable<SyntaxTrivia>? RemoveDirectivesFromTrivia(in SyntaxTriviaList trivia, 
																			 RegionDirectiveSearchMode? regionKindsToRemove)
		{
			if (trivia.Count == 0 || regionKindsToRemove == RegionDirectiveSearchMode.None)
				return null;

			var directiveTrivias = regionKindsToRemove.HasValue
				? trivia.GetRegionDirectiveLines(regionKindsToRemove.Value)
				: trivia.GetCompilerDirectives();

			if (directiveTrivias.Count == 0)
				return null;

			var newTriviaWithoutRegions = trivia.Except(directiveTrivias);
			return newTriviaWithoutRegions;
		}
	}
}
