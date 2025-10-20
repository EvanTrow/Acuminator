using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.Roslyn.CSharpVersion
{
    /// <summary>
    /// A helper class with utility methods related to the C# language version.
    /// </summary>
    public static class CSharpVersionUtils
	{
		// The current version of Roslyn doesn't support LanguageVersion.CSharp12 enum value, so we use the int value directly.
		// https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.languageversion?view=roslyn-dotnet-4.13.0
		private const int CSharp12 = 1200;

		/// <summary>
		/// The effective C# language version for the provided <paramref name="document"/>.
		/// </summary>
		/// <param name="document">The <see cref="Document"/> to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LanguageVersion? EffectiveCSharpVersion(this Document? document) =>
			(document?.Project.ParseOptions as CSharpParseOptions)?.LanguageVersion;

		/// <summary>
		/// The effective C# language version for the provided <paramref name="project"/>.
		/// </summary>
		/// <param name="project">The <see cref="Project"/> to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LanguageVersion? EffectiveCSharpVersion(this Project? project) =>
			(project?.ParseOptions as CSharpParseOptions)?.LanguageVersion;

		/// <summary>
		/// The effective C# language version for the provided <paramref name="syntaxTree"/>.
		/// </summary>
		/// <param name="syntaxTree">The <see cref="SyntaxTree"/> to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LanguageVersion? EffectiveCSharpVersion(this SyntaxTree? syntaxTree) =>
			(syntaxTree?.Options as CSharpParseOptions)?.LanguageVersion;

		/// <summary>
		/// Determines whether the effective C# language version is at least C# 12.
		/// </summary>
		/// <param name="effectiveLanguageVersion">The effective <see cref="LanguageVersion"/> to act on.</param>
		/// <returns>
		/// True if C# language version is at least C# 12.
		/// </returns>
		public static bool IsAtLeastCSharp12(this LanguageVersion? effectiveLanguageVersion)
		{
			// We can't guarantee that the language version is at least C# 12 if it's Default or Latest
			// because the latest version depends on the target framework and for .Net Framework it is lower.
			if (effectiveLanguageVersion is null or LanguageVersion.Default or LanguageVersion.Latest)	
				return false;

			return ((int)effectiveLanguageVersion.Value) >= CSharp12;
		}
	}
}
