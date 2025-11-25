
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.DiagnosticSuppression.CodeActions;
using Acuminator.Utilities.Roslyn.CodeActions;
using Acuminator.Utilities.Roslyn.Syntax;

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
			bool isInsideList = nodeToPlaceComment.Parent is BaseArgumentListSyntax or TypeArgumentListSyntax or BaseParameterListSyntax;

			var modifiedRoot = isInsideList
				? GetNewRootWithSuppressionCommentForArgumentNodeInList(root!, nodeToPlaceComment, suppressionComment)
				: GetNewRootWithSuppressionCommentForNonListNode(root!, nodeToPlaceComment, suppressionComment);

			return document.WithSyntaxRoot(modifiedRoot);
		}

		protected SyntaxNode GetNewRootWithSuppressionCommentForNonListNode(SyntaxNode root, SyntaxNode nodeToPlaceComment,
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

		protected SyntaxNode GetNewRootWithSuppressionCommentForArgumentNodeInList(SyntaxNode root, SyntaxNode nodeToPlaceComment,
																					string suppressionComment)
		{
			bool shouldAddNewLineBeforeComment = ShouldAddNewLineBeforeComment(nodeToPlaceComment);

			SyntaxTriviaList oldLeadingTrivia  = nodeToPlaceComment.GetLeadingTrivia();
			var whiteSpaceIndentationTrivia	   = GetWhiteSpaceTrivia(oldLeadingTrivia);

			var parentLeadingTrivia 				  = GetParentTrivia(nodeToPlaceComment);
			var whiteSpaceIndentationParentTrivia = GetWhiteSpaceTrivia(parentLeadingTrivia);

			bool oldTriviaStartsWithNewLine = 
				!shouldAddNewLineBeforeComment && oldLeadingTrivia.Count > 0 && oldLeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia);
			var suppressionCommentTriviasMutableList = new List<SyntaxTrivia>(capacity: 16);

			if (shouldAddNewLineBeforeComment || oldTriviaStartsWithNewLine)
			{
				suppressionCommentTriviasMutableList.Add(SyntaxFactory.CarriageReturnLineFeed);

				if (shouldAddNewLineBeforeComment)
				{
					suppressionCommentTriviasMutableList.AddRange(whiteSpaceIndentationParentTrivia);
					suppressionCommentTriviasMutableList.Add(SyntaxFactory.Tab);
					suppressionCommentTriviasMutableList.Add(SyntaxFactory.Tab);
				}
			}

			suppressionCommentTriviasMutableList.AddRange(whiteSpaceIndentationTrivia);
			suppressionCommentTriviasMutableList.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, suppressionComment));

			if (!oldTriviaStartsWithNewLine)
			{
				suppressionCommentTriviasMutableList.Add(SyntaxFactory.CarriageReturnLineFeed);

				if (shouldAddNewLineBeforeComment)
				{
					suppressionCommentTriviasMutableList.AddRange(whiteSpaceIndentationParentTrivia);
					suppressionCommentTriviasMutableList.Add(SyntaxFactory.Tab);
					suppressionCommentTriviasMutableList.Add(SyntaxFactory.Tab);
				}
			}

			suppressionCommentTriviasMutableList.AddRange(oldLeadingTrivia);

			var newLeadingTrivia = new SyntaxTriviaList(suppressionCommentTriviasMutableList);
			var nodeWithSuppressionComment = nodeToPlaceComment.WithLeadingTrivia(newLeadingTrivia);
			var modifiedRoot = root.ReplaceNode(nodeToPlaceComment, nodeWithSuppressionComment);

			return modifiedRoot;
		}

		private bool ShouldAddNewLineBeforeComment(SyntaxNode nodeToPlaceComment)
		{
			if (nodeToPlaceComment.Parent == null)  // in all uncertain cases add new line for safety
				return true;

			var nodeLocation  = nodeToPlaceComment.GetLocation();
			var nodeLineSpan  = nodeLocation.GetLineSpan();

			var parentLocation = nodeToPlaceComment.Parent.GetLocation();
			var parentLineSpan = parentLocation.GetLineSpan();

			// Node with a comment is on the same line as the parent list => need to add new line
			if (parentLineSpan.StartLinePosition.Line == nodeLineSpan.StartLinePosition.Line)
				return true;

			var arguments = GetArguments(nodeToPlaceComment.Parent);

			if (arguments?.Count is null or 0)
				return true;

			int nodeToPlaceCommentIndex = arguments.FindIndex(argNode => argNode.Equals(nodeToPlaceComment));

			// Node with a comment is first in the list and on a different line than the parent => no need to add new line
			if (nodeToPlaceCommentIndex <= 0)	
				return false;

			var previousArgument = arguments[nodeToPlaceCommentIndex - 1];
			var previousArgumentLocation = previousArgument.GetLocation();
			var previousArgumentLineSpan = previousArgumentLocation.GetLineSpan();

			// Node with a comment is on the same line as the previous argument => need to add new line
			return previousArgumentLineSpan.EndLinePosition.Line == nodeLineSpan.StartLinePosition.Line;
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

		private static IReadOnlyList<SyntaxNode>? GetArguments(SyntaxNode listNode) =>
			listNode switch
			{
				BaseArgumentListSyntax baseArgList		  => baseArgList.Arguments,
				BaseParameterListSyntax baseParameterList => baseParameterList.Parameters,
				TypeArgumentListSyntax typeArgumentList	  => typeArgumentList.Arguments,
				_ 										  => null
			};

		private static SyntaxTriviaList? GetParentTrivia(SyntaxNode nodeToPlaceComment)
		{
			SyntaxNode? nodeToGetTrivia = nodeToPlaceComment;

			while (nodeToGetTrivia != null && !SuppressionExtensions.ShouldStopSearchForSuppressionComment(nodeToGetTrivia))
			{
				nodeToGetTrivia = nodeToGetTrivia.Parent;
			}

			return nodeToGetTrivia?.GetLeadingTrivia();
		}

		private static List<SyntaxTrivia> GetWhiteSpaceTrivia(in SyntaxTriviaList? trivia)
		{
			if (trivia?.Count is null or 0)
				return [];

			return trivia.Value.Reverse()
							   .TakeWhile((in SyntaxTrivia t) => !t.IsKind(SyntaxKind.EndOfLineTrivia))
							   .Where(t => !t.IsDirective && t.IsKind(SyntaxKind.WhitespaceTrivia))
							   .Reverse()
							   .ToList(trivia.Value.Count);
		}
	}
}
