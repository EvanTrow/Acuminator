using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	public partial class NonPublicOrVirtualPXOverrideFix
	{
		private class NonPublicVirtualPXOverrideModifiersRewriter(bool changeVirtualKindToNonVirtual, bool changeModifierToPublic) : MethodModifiersRewriterBase
		{
			private readonly bool _changeVirtualKindToNonVirtual = changeVirtualKindToNonVirtual;
			private readonly bool _changeModifierToPublic = changeModifierToPublic;

			protected override bool ShouldRewriteModifiers(MethodDeclarationSyntax memberNode) => 
				base.ShouldRewriteModifiers(memberNode) && 
				(_changeModifierToPublic || _changeVirtualKindToNonVirtual);

			protected override IReadOnlyCollection<SyntaxKind> GetModifiersToAdd() =>
				_changeModifierToPublic
					? [SyntaxKind.PublicKeyword]
					: [];
			
			protected override bool ShouldModifierBeRemoved(in SyntaxToken modifier)
			{
				switch (modifier.Kind())
				{
					case SyntaxKind.PrivateKeyword:
					case SyntaxKind.ProtectedKeyword:
					case SyntaxKind.InternalKeyword:
						return _changeModifierToPublic;

					case SyntaxKind.VirtualKeyword:
						return _changeVirtualKindToNonVirtual;

					default:
						return false;
				}
			}
		}
	}
}
