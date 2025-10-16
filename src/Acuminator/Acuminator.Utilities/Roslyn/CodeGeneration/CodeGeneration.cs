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

		public static TNode CopyRegionsFromTrivia<TNode>(TNode nodeToCopyTrivia, in SyntaxTriviaList triviaWithRegions,
														 bool copyBeforeNode, bool insertCopiedRegionsAfterNodeTrivia)
		where TNode : SyntaxNode
		{
			nodeToCopyTrivia.ThrowOnNull();

			var regionTrivias = triviaWithRegions.GetRegionDirectiveLines();

			if (regionTrivias.Count == 0)
				return nodeToCopyTrivia;

			if (copyBeforeNode)
			{
				var bqlFieldNodeLeadingTrivia = nodeToCopyTrivia.GetLeadingTrivia();
				var newBqlFieldNodeTrivia = insertCopiedRegionsAfterNodeTrivia
					? bqlFieldNodeLeadingTrivia.AddRange(regionTrivias)
					: bqlFieldNodeLeadingTrivia.InsertRange(0, regionTrivias);

				return nodeToCopyTrivia.WithLeadingTrivia(newBqlFieldNodeTrivia);
			}
			else
			{
				var bqlFieldNodeTrailingTrivia = nodeToCopyTrivia.GetTrailingTrivia();
				var newBqlFieldNodeTrivia = insertCopiedRegionsAfterNodeTrivia
					? bqlFieldNodeTrailingTrivia.AddRange(regionTrivias)
					: bqlFieldNodeTrailingTrivia.InsertRange(0, regionTrivias);

				return nodeToCopyTrivia.WithTrailingTrivia(newBqlFieldNodeTrivia);
			}
		}

		/// <summary>
		/// Removes the regions from the type member node leading trivia.
		/// </summary>
		/// <param name="member">The type member node.</param>
		/// <returns>
		/// Type member node with removed regions from leading trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(member))]
		public static MemberDeclarationSyntax? RemoveRegionsFromLeadingTrivia(MemberDeclarationSyntax? member)
		{
			if (member == null)
				return member;

			var leadingTrivia	 = member.GetLeadingTrivia();
			var newLeadingTrivia = RemoveRegionsFromTrivia(leadingTrivia);

			return newLeadingTrivia != null
				? member.WithLeadingTrivia(newLeadingTrivia)
				: member;
		}

		/// <summary>
		/// Removes the regions from the type member node trailing trivia.
		/// </summary>
		/// <param name="member">The type member node.</param>
		/// <returns>
		/// Type member node with removed regions from trailing trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(member))]
		public static MemberDeclarationSyntax? RemoveRegionsFromTrailingTrivia(MemberDeclarationSyntax? member)
		{
			if (member == null)
				return member;

			var trailingTrivia	  = member.GetTrailingTrivia();
			var newTrailingTrivia = RemoveRegionsFromTrivia(trailingTrivia);

			return newTrailingTrivia != null
				? member.WithTrailingTrivia(newTrailingTrivia)
				: member;
		}

		public static IEnumerable<SyntaxTrivia>? RemoveRegionsFromTrivia(in SyntaxTriviaList trivia)
		{
			if (trivia.Count == 0)
				return null;

			var regionTrivias = trivia.GetRegionDirectiveLines();

			if (regionTrivias.Count == 0)
				return null;

			var newTriviaWithoutRegions = trivia.Except(regionTrivias);
			return newTriviaWithoutRegions;
		}
	}
}
