using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers
{
	public partial class ForbidPrivateEventHandlersFix
	{
		private class PrivateEventHandlersModifiersRewriter(SyntaxKind accessibilityModifier, bool addVirtual) : MethodModifiersRewriterBase
		{
			private readonly SyntaxKind _accessibilityModifier = accessibilityModifier;
			private readonly bool _addVirtual = addVirtual;

			protected override IReadOnlyCollection<SyntaxKind> GetModifiersToAdd() =>
				_addVirtual
					? [_accessibilityModifier, SyntaxKind.VirtualKeyword]
					: [_accessibilityModifier];

			protected override bool ShouldModifierBeRemoved(in SyntaxToken modifier)
			{
				switch (modifier.Kind())
				{
					case SyntaxKind.PrivateKeyword:
					case SyntaxKind.PublicKeyword:
					case SyntaxKind.ProtectedKeyword:
					case SyntaxKind.InternalKeyword:
					case SyntaxKind.VirtualKeyword:
						return true;
					default:
						return false;
				}
			}
		}
	}
}
