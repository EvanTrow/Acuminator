using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// A base class for rewriting type member modifiers.
	/// </summary>
	public abstract class TypeMemberModifiersRewriterBase<TMemberNode>
	where TMemberNode : MemberDeclarationSyntax
	{
		public TMemberNode RewriteModifiers(TMemberNode memberNode)
		{
			if (!ShouldRewriteModifiers(memberNode.CheckIfNull()))
				return memberNode;

			var modifiers = memberNode.Modifiers;
			SyntaxToken firstModifier = memberNode.Modifiers.FirstOrDefault();
			bool hasModifiers = firstModifier != default;
			bool shouldRemoveFirstModifier = ShouldModifierBeRemoved(firstModifier);

			var modifierKindsToAdd = GetModifiersToAdd();
			var modifiersToAdd = modifierKindsToAdd.Count > 0
				? modifierKindsToAdd.Select(SyntaxFactory.Token).ToList(modifierKindsToAdd.Count)
				: [];

			var newMemberModifiers = new List<SyntaxToken>(capacity: modifiersToAdd.Count + 4);

			if (modifiersToAdd.Count > 0)
			{
				var newFirstModifier = modifiersToAdd[0];
				var firstNewModifierWithCopiedTrivia = 
					CopyTriviaForNewFirstModifier(newFirstModifier, hasModifiers, shouldRemoveFirstModifier, firstModifier, memberNode);

				newMemberModifiers.Add(firstNewModifierWithCopiedTrivia);
				newMemberModifiers.AddRange(modifiersToAdd.Skip(1));
			}

			if (hasModifiers)
			{
				var modifiersToKeep = GetModifiersToKeep(firstModifier, modifiers, shouldRemoveFirstModifier, 
														 hasNewModifiers: modifiersToAdd.Count > 0);
				newMemberModifiers.AddRange(modifiersToKeep);
			}

			var newModifiersTokenList = SyntaxFactory.TokenList(newMemberModifiers);
			var newMemberNode = ReplaceModifiers(memberNode, newModifiersTokenList);

			if (!hasModifiers && modifiersToAdd.Count > 0)
			{
				// if there are no modifiers in the original method, we took over the first leading trivia (for methods it is taken from the return type) 
				// to the new modifier token. Now we need to remove the leading trivia from the token from which it was taken (the return type).
				newMemberNode = RemoveFirstLeadingTriviaAfterModifiers(newMemberNode);
			}

			return newMemberNode;
		}

		protected virtual bool ShouldRewriteModifiers(TMemberNode memberNode) => true;

		protected SyntaxToken CopyTriviaForNewFirstModifier(in SyntaxToken newFirstModifier, bool hasModifiers, bool shouldRemoveFirstModifier,
															in SyntaxToken oldFirstModifier, TMemberNode memberNode)
		{
			var modifiers = memberNode.Modifiers;

			// Preserve the leading trivia of the first token, if it exists. If not, take over the leading trivia from the return type.
			SyntaxToken newFirstModifierWithTrivia;

			if (hasModifiers)
			{
				SyntaxTriviaList leadingTriviaToAdd = oldFirstModifier.LeadingTrivia;
				SyntaxTriviaList trailingTriviaToAdd = shouldRemoveFirstModifier
					? oldFirstModifier.TrailingTrivia
					: SyntaxTriviaList.Create(SyntaxFactory.Space);

				// Get the leading and trailing trivia of the modifiers that are about to be removed.
				var modifiersToBeRemoved = modifiers.Skip(1)    // skip the first token, as the trivia from it is always added
													.Where(m => ShouldModifierBeRemoved(m))
													.ToList();
				if (modifiersToBeRemoved.Count > 0)
				{
					var leadingTriviaFromRemovedModifiers = modifiersToBeRemoved.Where(m => m.HasLeadingTrivia)
																				 .SelectMany(m => m.LeadingTrivia);
					var trailingTriviaFromRemovedModifiers = modifiersToBeRemoved.Where(m => m.HasTrailingTrivia)
																				 .SelectMany(m => m.TrailingTrivia);
					if (leadingTriviaFromRemovedModifiers.Any())
						leadingTriviaToAdd = leadingTriviaToAdd.AddRange(leadingTriviaFromRemovedModifiers);

					if (trailingTriviaFromRemovedModifiers.Any())
						trailingTriviaToAdd = trailingTriviaToAdd.AddRange(trailingTriviaFromRemovedModifiers);
				}

				var formattedLeadingTriviaToAdd = RemoveSequantialWhiteSpaces(leadingTriviaToAdd);
				var formattedTrailingTriviaToAdd = RemoveSequantialWhiteSpaces(trailingTriviaToAdd);
				newFirstModifierWithTrivia = newFirstModifier.WithLeadingTrivia(formattedLeadingTriviaToAdd)
															 .WithTrailingTrivia(formattedTrailingTriviaToAdd);
			}
			else
			{
				var firstLeadingTriviaAfterModifiers = GetFirstLeadingTriviaAfterModifiers(memberNode);
				newFirstModifierWithTrivia = newFirstModifier.WithLeadingTrivia(firstLeadingTriviaAfterModifiers);
			}

			return newFirstModifierWithTrivia;
		}

		private static IEnumerable<SyntaxTrivia> RemoveSequantialWhiteSpaces(SyntaxTriviaList syntaxTrivias)
		{
			SyntaxTrivia? previousTrivia = null;

			foreach (var trivia in syntaxTrivias)
			{
				if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia) || previousTrivia == null || !previousTrivia.Value.IsKind(SyntaxKind.WhitespaceTrivia))
					yield return trivia;

				previousTrivia = trivia;
			}
		}

		private IEnumerable<SyntaxToken> GetModifiersToKeep(in SyntaxToken firstModifier, in SyntaxTokenList modifiers, bool shouldRemoveFirstModifier,
															bool hasNewModifiers)
		{
			if (!shouldRemoveFirstModifier)
			{
				var modifiersToKeep = FilterModifiers(modifiers, includeFirstModifier: false);

				if (hasNewModifiers)
				{
					// If the previously first token was not a modifier to be removed, and there will be new first modifiers, 
					// then we need to add it back _without_ the leading trivia. That's why we add it separately first, and then we add the rest.
					var firstModifierWithoutLeadingTrivia = firstModifier.WithLeadingTrivia(default(SyntaxTriviaList));
					return modifiersToKeep.PrependItem(firstModifierWithoutLeadingTrivia);
				}
				else
					return modifiersToKeep.PrependItem(firstModifier);
			}
			else
				return FilterModifiers(modifiers, includeFirstModifier: true);
		}

		private IEnumerable<SyntaxToken> FilterModifiers(in SyntaxTokenList modifiers, bool includeFirstModifier)
		{
			if (includeFirstModifier)
				return modifiers.Where(m => !ShouldModifierBeRemoved(m));

			switch (modifiers.Count)
			{
				case <= 1:
					return [];
				case 2:
					SyntaxToken secondModifier = modifiers[1];
					return ShouldModifierBeRemoved(secondModifier)
						? []
						: [secondModifier];
				default:
					return SkipFirstModifiersAndFilterMoreThanTwoModifiers(modifiers);
			}

			//-----------------------------Local Function-------------------------------------------
			IEnumerable<SyntaxToken> SkipFirstModifiersAndFilterMoreThanTwoModifiers(SyntaxTokenList modifiersToFilter)
			{
				for (int i = 1; i < modifiersToFilter.Count; i++)
				{
					SyntaxToken modifier = modifiersToFilter[i];

					if (!ShouldModifierBeRemoved(modifier))
						yield return modifier;
				}
			}
		}

		protected abstract IReadOnlyCollection<SyntaxKind> GetModifiersToAdd();

		protected abstract bool ShouldModifierBeRemoved(in SyntaxToken modifier);

		protected abstract SyntaxTriviaList GetFirstLeadingTriviaAfterModifiers(TMemberNode memberNode);

		protected abstract TMemberNode RemoveFirstLeadingTriviaAfterModifiers(TMemberNode memberNode);

		protected abstract TMemberNode ReplaceModifiers(TMemberNode memberNode, in SyntaxTokenList newModifiersTokenList);
	}
}
