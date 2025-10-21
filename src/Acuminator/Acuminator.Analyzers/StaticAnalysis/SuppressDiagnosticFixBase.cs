
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.DiagnosticSuppression.CodeActions;
using Acuminator.Utilities.Roslyn.CodeActions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class SuppressDiagnosticFixBase : CodeFixProvider
	{
		protected const string PX1007_ID = "PX1007";
		protected const string SuppressionCommentFormat = @"// Acuminator disable once {0} {1} {2}";
		protected static ImmutableArray<string> AllCollectedFixableDiagnosticIds { get; }

		protected static HashSet<string> DiagnosticIdsWithFixAllEnabled { get; } = new(StringComparer.OrdinalIgnoreCase)
		{
			PX1007_ID
		};

		static SuppressDiagnosticFixBase()
		{
			Type diagnosticsType = typeof(Descriptors);
			var propertiesInfo = diagnosticsType.GetRuntimeProperties();

			AllCollectedFixableDiagnosticIds = propertiesInfo
				.Where(property => property.PropertyType == typeof(DiagnosticDescriptor))
				.Select(property =>
				{
					var descriptor = property.GetValue(null) as DiagnosticDescriptor;
					return descriptor?.Id;
				})
				.Where(id => id != null)
				.ToImmutableArray()!;
		}

		/// <summary>
		/// Derived code fixes need to specify Fix All provider explicitly.
		/// </summary>
		/// <returns>
		/// The Fix All provider.
		/// </returns>
		public abstract override FixAllProvider? GetFixAllProvider();

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			foreach (var diagnostic in context.Diagnostics)
			{
				RegisterCodeActionForDiagnostic(diagnostic, context);
			}

			return Task.CompletedTask;
		}

		protected virtual void RegisterCodeActionForDiagnostic(Diagnostic diagnostic, CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CodeAction? groupCodeAction = GetCodeActionToRegister(diagnostic, context);

			if (groupCodeAction != null)
			{
				context.RegisterCodeFix(groupCodeAction, diagnostic);
			}
		}

		protected virtual CodeAction? GetCodeActionToRegister(Diagnostic diagnostic, CodeFixContext context)
		{
			if (!SuppressionManager.CheckIfInstanceIsInitialized(throwOnNotInitialized: false))
			{
				return GetSuppressWithCommentCodeAction(diagnostic, context, isNested: false);
			}

			string groupCodeActionNameFormat = nameof(Resources.SuppressDiagnosticGroupCodeActionTitle).GetLocalized().ToString();
			string groupCodeActionName = string.Format(groupCodeActionNameFormat, diagnostic.Id);

			CodeAction suppressWithCommentCodeAction = GetSuppressWithCommentCodeAction(diagnostic, context, isNested: true);
			CodeAction suppressWithSuppressionFileCodeAction = GetSuppressWithSuppressionFileCodeAction(diagnostic, context, isNested: true);
			var suppressionCodeActions = ImmutableArray.CreateBuilder<CodeAction>(initialCapacity: 2);

			if (suppressWithCommentCodeAction != null)
			{
				suppressionCodeActions.Add(suppressWithCommentCodeAction);
			}

			if (suppressWithSuppressionFileCodeAction != null)
			{
				suppressionCodeActions.Add(suppressWithSuppressionFileCodeAction);
			}

			// Use reflection based factory to create group code action with nested actions and custom priority.
			// Setting code action priority is impossible via Roslyn public API.
			var groupCodeAction =
				CodeActionWithNestedActionsFabric.CreateCodeActionWithNestedActions(groupCodeActionName, suppressionCodeActions.ToImmutable()) ??
				CodeAction.Create(groupCodeActionName, suppressionCodeActions.ToImmutable(), isInlinable: false);

			return groupCodeAction;
		}

		protected virtual CodeAction GetSuppressWithCommentCodeAction(Diagnostic diagnostic, CodeFixContext context, bool isNested)
		{
			string commentCodeActionName;

			if (isNested)
			{
				commentCodeActionName = nameof(Resources.SuppressDiagnosticWithCommentNestedCodeActionTitle).GetLocalized().ToString();
			}
			else
			{
				commentCodeActionName = nameof(Resources.SuppressDiagnosticWithCommentNonNestedCodeActionTitle)
												.GetLocalized(diagnostic.Id).ToString();
			}

			return CodeAction.Create(commentCodeActionName,
									 cToken => AddSuppressionCommentAsync(context, diagnostic, cToken),
									 equivalenceKey: commentCodeActionName + diagnostic.Id);
		}

		protected virtual CodeAction GetSuppressWithSuppressionFileCodeAction(Diagnostic diagnostic, CodeFixContext context, bool isNested)
		{
			string suppressionFileCodeActionName = nameof(Resources.SuppressDiagnosticInSuppressionFileCodeActionTitle).GetLocalized().ToString();
			return new SuppressWithSuppressionFileCodeAction(context, diagnostic, suppressionFileCodeActionName,
															 equivalenceKey: suppressionFileCodeActionName + diagnostic.Id);
		}

		protected virtual async Task<Document> AddSuppressionCommentAsync(CodeFixContext context, Diagnostic diagnostic,
																		  CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var document = context.Document;
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? reportedNode = root?.FindNode(context.Span);

			if (diagnostic == null || reportedNode == null)
				return document;

			SyntaxNode? nodeToPlaceComment = GetNodeToPlaceComment(reportedNode);

			if (nodeToPlaceComment == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var (diagnosticShortName, diagnosticJustification) = GetDiagnosticShortNameAndJustification(diagnostic);

			if (diagnosticShortName.IsNullOrWhiteSpace())
				return document;

			string suppressionComment = string.Format(SuppressionCommentFormat, diagnostic.Id, diagnosticShortName, diagnosticJustification);
			bool isInsideList = nodeToPlaceComment.Parent is BaseArgumentListSyntax or TypeArgumentListSyntax;

			var modifiedRoot = isInsideList
				? GetNewRootWithSuppressionCommentForArgumentNodeInList(root!, nodeToPlaceComment, suppressionComment)
				: GetNewRootWithSuppressionCommentForNonListNode(root!, nodeToPlaceComment, suppressionComment);

			return document.WithSyntaxRoot(modifiedRoot);
		}

		protected virtual SyntaxNode GetNewRootWithSuppressionCommentForArgumentNodeInList(SyntaxNode root, SyntaxNode nodeToPlaceComment,
																							string suppressionComment)
		{
			var suppressionCommentTrivias = new SyntaxTriviaList
			(
				SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, suppressionComment),
				SyntaxFactory.CarriageReturnLineFeed
			);

			SyntaxTriviaList oldLeadingTrivia = nodeToPlaceComment.GetLeadingTrivia();
			SyntaxTriviaList newLeadingTrivia;

			if (oldLeadingTrivia.Count > 0)
			{
				var whiteSpaceIndentationTrivia = oldLeadingTrivia.TakeWhile((in SyntaxTrivia t) => !t.IsKind(SyntaxKind.EndOfLineTrivia))
																  .Where(t => !t.IsDirective && t.IsKind(SyntaxKind.WhitespaceTrivia));
				var mutableTriviaList = whiteSpaceIndentationTrivia.ToList(capacity: oldLeadingTrivia.Count);
				mutableTriviaList.AddRange(suppressionCommentTrivias);
				mutableTriviaList.AddRange(oldLeadingTrivia);

				newLeadingTrivia = new SyntaxTriviaList(mutableTriviaList);
			}
			else
				newLeadingTrivia = suppressionCommentTrivias;

			var nodeWithSuppressionComment = nodeToPlaceComment.WithLeadingTrivia(newLeadingTrivia);
			var modifiedRoot = root.ReplaceNode(nodeToPlaceComment, nodeWithSuppressionComment);

			return modifiedRoot;
		}

		protected virtual SyntaxNode GetNewRootWithSuppressionCommentForNonListNode(SyntaxNode root, SyntaxNode nodeToPlaceComment,
																					string suppressionComment)
		{
			var suppressionCommentTrivias = new SyntaxTrivia[]
			{
				SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, suppressionComment),
				SyntaxFactory.ElasticEndOfLine(string.Empty)
			};

			SyntaxTriviaList leadingTrivia = nodeToPlaceComment.GetLeadingTrivia();
			SyntaxNode? modifiedRoot;

			if (leadingTrivia.Count > 0)
				modifiedRoot = root.InsertTriviaAfter(leadingTrivia.Last(), suppressionCommentTrivias);
			else
			{
				var nodeWithSuppressionComment = nodeToPlaceComment.WithLeadingTrivia(suppressionCommentTrivias);
				modifiedRoot = root.ReplaceNode(nodeToPlaceComment, nodeWithSuppressionComment);
			}

			return modifiedRoot;
		}

		protected (string? DiagnosticShortName, string? DiagnosticJustification) GetDiagnosticShortNameAndJustification(Diagnostic diagnostic)
		{
			string[]? customTags = diagnostic.Descriptor.CustomTags?.ToArray();

			if (customTags.IsNullOrEmpty())
				return default;

			string diagnosticShortName = customTags[0];
			string diagnosticJustification = customTags.Length > 1
				? customTags[1]
				: DiagnosticsDefaultJustification.Default;

			return (diagnosticShortName, diagnosticJustification);
		}

		protected SyntaxNode? GetNodeToPlaceComment(SyntaxNode reportedNode)
		{
			SyntaxNode? nodeToPlaceComment = reportedNode;
			while (nodeToPlaceComment != null && !SuppressionExtensions.CanNodeContainSuppressionComment(nodeToPlaceComment))
			{
				nodeToPlaceComment = nodeToPlaceComment.Parent;
			}

			return nodeToPlaceComment;
		}
	}
}
