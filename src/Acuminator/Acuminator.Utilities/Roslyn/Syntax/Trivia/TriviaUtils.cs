using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax.Trivia
{
	public static class TriviaUtils
	{
		/// <summary>
		/// Check if <paramref name="trivia"/> represents a comment.
		/// </summary>
		/// <param name="trivia">The trivia to act on.</param>
		/// <returns>
		/// <see langword="true"/> if trivia is a comment, <see langword="false"/> if not.
		/// </returns>
		public static bool IsCommentTrivia(this in SyntaxTrivia trivia) =>
			trivia.Kind() is SyntaxKind.SingleLineDocumentationCommentTrivia or
							 SyntaxKind.MultiLineDocumentationCommentTrivia or
							 SyntaxKind.SingleLineCommentTrivia or
							 SyntaxKind.MultiLineCommentTrivia;

		public static bool ContainsNewLine(this in SyntaxTriviaList trivias)
		{
			for (int i = 0; i < trivias.Count; i++)
			{
				SyntaxTrivia trivia = trivias[i];

				if (trivia.Kind() is SyntaxKind.EndOfLineTrivia or SyntaxKind.XmlTextLiteralNewLineToken)
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SyntaxTrivia ToSingleLineComment(this string? commentContent)
		{
			const string commentPrefix = "//";
			string comment = commentContent.IsNullOrWhiteSpace()
				? commentPrefix
				: commentPrefix + " " + commentContent.Trim();

			return SyntaxFactory.Comment(comment);
		}

		public static List<SyntaxTrivia> GetCompilerDirectives(this in SyntaxTriviaList trivias)
		{
			if (trivias.Count == 0)
				return [];

			var compilerTrivias = new List<SyntaxTrivia>(2);
			SyntaxTrivia? previousTrivia = null;

			for (int i = 0; i < trivias.Count; i++)
			{
				var trivia = trivias[i];

				if (trivia.IsDirective)
				{
					if (previousTrivia.HasValue && previousTrivia.Value.IsKind(SyntaxKind.WhitespaceTrivia))
						compilerTrivias.Add(previousTrivia.Value);

					compilerTrivias.Add(trivia);
				}

				previousTrivia = trivia;
			}

			return compilerTrivias;
		}

		public static List<SyntaxTrivia> GetRegionDirectiveLines(this in SyntaxTriviaList trivias, 
																 RegionDirectiveSearchMode regionDirectiveSearchMode)
		{
			if (trivias.Count == 0 || regionDirectiveSearchMode == RegionDirectiveSearchMode.None)
				return [];

			var regionTrivias = new List<SyntaxTrivia>(2);
			SyntaxTrivia? previousTrivia = null;

			for (int i = 0; i < trivias.Count; i++)
			{
				var trivia = trivias[i];
				bool includeTrivia = regionDirectiveSearchMode switch
				{
					RegionDirectiveSearchMode.StartRegion => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia),
					RegionDirectiveSearchMode.EndRegion   => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia),
					RegionDirectiveSearchMode.AllRegions  => trivia.Kind() is SyntaxKind.RegionDirectiveTrivia or 
																			  SyntaxKind.EndRegionDirectiveTrivia,
					_ 									  => false,
				};

				if (includeTrivia)
				{
					if (previousTrivia.HasValue && previousTrivia.Value.IsKind(SyntaxKind.WhitespaceTrivia))
						regionTrivias.Add(previousTrivia.Value);

					regionTrivias.Add(trivia);
				}

				previousTrivia = trivia;
			}

			return regionTrivias;
		}
	}
}
