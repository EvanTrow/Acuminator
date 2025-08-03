using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public partial class NonPublicOrVirtualPXOverrideFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1097_PXOverrideMethodMustBePublicNonVirtual.Id
			);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var virtualityKind = GetVirtualityKindFromProperties(diagnostic);
			bool virtualityKindAllowsChanges = !virtualityKind.HasValue ||
				virtualityKind is MemberVirtualityKind.None or MemberVirtualityKind.Abstract or MemberVirtualityKind.Virtual;

			if (!virtualityKindAllowsChanges)
				return Task.CompletedTask;

			bool changeVirtualKindToNonVirtual = virtualityKind == MemberVirtualityKind.Virtual;
			bool changeModifierToPublic = diagnostic.IsFlagSet(PXOverrideDiagnosticProperties.IsNonPublicPatchMethod);

			if (!changeModifierToPublic && !changeVirtualKindToNonVirtual)
				return Task.CompletedTask;

			if (!diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.PatchMethodName, out string? patchMethodName) ||
				patchMethodName.IsNullOrWhiteSpace())
			{
				patchMethodName = string.Empty;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			string title = nameof(Resources.PX1097Fix).GetLocalized(patchMethodName).ToString();
			var document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cToken => ChangePatchMethodToPublicNonVirtual(document, context.Span, changeVirtualKindToNonVirtual,
																							 changeModifierToPublic, cToken),
											   equivalenceKey: nameof(Resources.PX1097Fix));
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private MemberVirtualityKind? GetVirtualityKindFromProperties(Diagnostic diagnostic)
		{
			if (diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.PatchMethodVirtualityKind, out string? virtualityKindStr) &&
				!virtualityKindStr.IsNullOrWhiteSpace() &&
				Enum.TryParse(virtualityKindStr, ignoreCase: true, out MemberVirtualityKind virtualityKind))
			{
				return virtualityKind;
			}

			return null;
		}

		private static async Task<Document> ChangePatchMethodToPublicNonVirtual(Document document, TextSpan span, bool changeVirtualKindToNonVirtual,
																				bool changeModifierToPublic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var patchMethodNode = root?.FindNode(span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (patchMethodNode == null)
				return document;

			var modifiersRewriter  = new NonPublicVirtualPXOverrideModifiersRewriter(changeVirtualKindToNonVirtual, changeModifierToPublic);
			var newPatchMethodNode = modifiersRewriter.RewriteModifiers(patchMethodNode);

			if (ReferenceEquals(newPatchMethodNode, patchMethodNode))
				return document;

			var newRoot = root!.ReplaceNode(patchMethodNode, newPatchMethodNode);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
