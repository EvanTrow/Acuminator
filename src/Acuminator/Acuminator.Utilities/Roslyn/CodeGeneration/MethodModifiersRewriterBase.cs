using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// A base class for rewriting method modifiers.
	/// </summary>
	public abstract class MethodModifiersRewriterBase : TypeMemberModifiersRewriterBase<MethodDeclarationSyntax>
	{
		protected override MethodDeclarationSyntax ReplaceModifiers(MethodDeclarationSyntax memberNode, in SyntaxTokenList newModifiersTokenList) =>
			memberNode.WithModifiers(newModifiersTokenList);

		protected override SyntaxTriviaList GetFirstLeadingTriviaAfterModifiers(MethodDeclarationSyntax memberNode) =>
			memberNode.ReturnType.GetLeadingTrivia();

		protected override MethodDeclarationSyntax RemoveFirstLeadingTriviaAfterModifiers(MethodDeclarationSyntax memberNode) =>
			memberNode.WithReturnType(memberNode.ReturnType.WithoutLeadingTrivia());
	}
}
