using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public partial class ForbidPrivateEventHandlersFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.Id,
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.Id,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.IsRegisteredForCodeFix(considerRegisteredByDefault: false))
				return Task.CompletedTask;

			var isContainingTypeSealed = diagnostic.IsFlagSet(PX1077DiagnosticProperty.IsContainingTypeSealed);
			var addVirtualModifier	   = diagnostic.IsFlagSet(PX1077DiagnosticProperty.AddVirtualModifier);

			var accessibilityModifier = isContainingTypeSealed
				? SyntaxKind.PublicKeyword
				: SyntaxKind.ProtectedKeyword;

			var modifierFormatArg = ForbidPrivateEventHandlersAnalyzer.GetModifiersText(isPublic: isContainingTypeSealed, addVirtualModifier);
			var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(modifierFormatArg).ToString();

			context.CancellationToken.ThrowIfCancellationRequested();

			var codeFixAction = CodeAction.Create(
				makeProtectedTitle,
				cToken => ChangeAccessibilityModifierAsync(context.Document, context.Span, accessibilityModifier, addVirtualModifier, cToken),
				equivalenceKey: Resources.PX1077Fix);

			context.RegisterCodeFix(codeFixAction, diagnostic);
			return Task.CompletedTask;
		}

		private static async Task<Document> ChangeAccessibilityModifierAsync(Document document, TextSpan span, SyntaxKind accessibilityModifier,
																			 bool addVirtual, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root?.FindNode(span) is not MethodDeclarationSyntax eventHandler)
				return document;

			var modifiersRewriter = new PrivateEventHandlersModifiersRewriter(accessibilityModifier, addVirtual);
			var newEventHandler = modifiersRewriter.RewriteModifiers(eventHandler);
			var newRoot = root.ReplaceNode(eventHandler, newEventHandler);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
