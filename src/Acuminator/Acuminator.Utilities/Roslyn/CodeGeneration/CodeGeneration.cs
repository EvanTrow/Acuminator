using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

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

			bool alreadyHasUsing = root.Usings.Any(usingDirective => namespaceName == usingDirective.Name?.ToString());

			if (alreadyHasUsing)
				return root;

			bool isSystemNamespace = namespaceName == NamespaceNames.System ||
									 namespaceName.StartsWith($"{NamespaceNames.System}.", StringComparison.Ordinal);
			var usingDirective = UsingDirective(
									ParseName(namespaceName));

			if (isSystemNamespace && insertSystemNamespaceFirst)
			{
				var newUsings =	root.Usings.Insert(0, usingDirective);
				return root.WithUsings(newUsings);
			}
			else
				return root.AddUsings(usingDirective);
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
											copyOnlyRegions: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TNode CopyRegionsFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithRegions,
														 bool copyBeforeNode, bool insertCopiedRegionsAfterNodeTrivia)
		where TNode : SyntaxNode
		{
			return CopyDirectivesFromTrivia(nodeToCopyTrivia, triviaWithRegions, copyBeforeNode, insertCopiedRegionsAfterNodeTrivia,
											copyOnlyRegions: true);
		}

		private static TNode CopyDirectivesFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithDirectives,
														 bool copyBeforeNode, bool insertCopiedTriviaAfterNodeTrivia,
														 bool copyOnlyRegions)
		where TNode : SyntaxNode
		{
			nodeToCopyTrivia.ThrowOnNull();

			var directiveTrivias = copyOnlyRegions
				? triviaWithDirectives.GetRegionDirectiveLines()
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
		/// <param name="node">The syntax node to act on.</param>
		/// <returns>
		/// The node with removed regions from leading trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(node))]
		public static TNode? RemoveRegionsFromLeadingTrivia<TNode>(this TNode? node)
		where TNode : SyntaxNode
		{
			if (node == null)
				return node;

			var leadingTrivia	 = node.GetLeadingTrivia();
			var newLeadingTrivia = RemoveRegionsFromTrivia(leadingTrivia);

			return newLeadingTrivia != null
				? node.WithLeadingTrivia(newLeadingTrivia)
				: node;
		}

		/// <summary>
		/// Removes the regions from the <paramref name="node"/>'s trailing trivia.
		/// </summary>
		/// <param name="node">The syntax node to act on.</param>
		/// <returns>
		/// The node with removed regions from trailing trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(node))]
		public static TNode? RemoveRegionsFromTrailingTrivia<TNode>(this TNode? node)
		where TNode : SyntaxNode
		{
			if (node == null)
				return node;

			var trailingTrivia	  = node.GetTrailingTrivia();
			var newTrailingTrivia = RemoveRegionsFromTrivia(trailingTrivia);

			return newTrailingTrivia != null
				? node.WithTrailingTrivia(newTrailingTrivia)
				: node;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia>? RemoveRegionsFromTrivia(in SyntaxTriviaList trivia) =>
			RemoveDirectivesFromTrivia(trivia, removeOnlyRegions: true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia>? RemoveCompilerDirectivesFromTrivia(in SyntaxTriviaList trivia) =>
			RemoveDirectivesFromTrivia(trivia, removeOnlyRegions: false);

		private static IEnumerable<SyntaxTrivia>? RemoveDirectivesFromTrivia(in SyntaxTriviaList trivia, bool removeOnlyRegions)
		{
			if (trivia.Count == 0)
				return null;

			var directiveTrivias = removeOnlyRegions 
				? trivia.GetRegionDirectiveLines()
				: trivia.GetCompilerDirectives();

			if (directiveTrivias.Count == 0)
				return null;

			var newTriviaWithoutRegions = trivia.Except(directiveTrivias);
			return newTriviaWithoutRegions;
		}
	}
}
