using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax.Trivia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public partial class AddXmlDocCommentWithReferenceToBaseMethodFix : PXCodeFixProvider
	{
		private const string XmlDocCommentOverridesPrefix = "Overrides";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.Id
			);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.BaseMethodDocCommentId, out var baseMethodDocCommentId) ||
				baseMethodDocCommentId.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			string title = nameof(Resources.PX1098Fix).GetLocalized().ToString();
			var document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cToken => AddXmlDocCommentToPatchMethod(document, baseMethodDocCommentId, context.Span, cToken),
											   equivalenceKey: nameof(Resources.PX1097Fix));
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static async Task<Document> AddXmlDocCommentToPatchMethod(Document document, string baseMethodDocCommentId, TextSpan span,
																		  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var patchMethodNode = root?.FindNode(span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (patchMethodNode == null)
				return document;

			var newPatchMethodNode = AddXmlDocCommentToPatchMethod(patchMethodNode, baseMethodDocCommentId);

			if (newPatchMethodNode == null || ReferenceEquals(patchMethodNode, newPatchMethodNode))
				return document;

			var newRoot = root!.ReplaceNode(patchMethodNode, newPatchMethodNode);
			var modifiedDocument = document.WithSyntaxRoot(newRoot);
			var simplifiedDocument = await Simplifier.ReduceAsync(modifiedDocument, cancellationToken: cancellation)
													 .ConfigureAwait(false);
			return simplifiedDocument;
		}

		private static MethodDeclarationSyntax? AddXmlDocCommentToPatchMethod(MethodDeclarationSyntax patchMethodNode, string baseMethodDocCommentId)
		{
			if (patchMethodNode.AttributeLists.Count > 0)
			{
				var firstAttributeList = patchMethodNode.AttributeLists[0];
				var tokenWithTrivia = AddXmlDocCommentToToken(firstAttributeList.OpenBracketToken, baseMethodDocCommentId);

				if (tokenWithTrivia == null)
					return null;

				var newAttributeLists =
					patchMethodNode.AttributeLists.RemoveAt(0)
												  .Insert(0, firstAttributeList.WithOpenBracketToken(tokenWithTrivia.Value));
				var newPatchMethodNode = patchMethodNode.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Simplifier.Annotation);
				return newPatchMethodNode;
			}

			var modifiers = patchMethodNode.Modifiers;

			if (modifiers.Count > 0)
			{
				var firstModifier = modifiers[0];
				var tokenWithTrivia = AddXmlDocCommentToToken(firstModifier, baseMethodDocCommentId);

				if (tokenWithTrivia == null)
					return null;

				var newModifiers = modifiers.RemoveAt(0)
											.Insert(0, tokenWithTrivia.Value);
				var newPatchMethodNode = patchMethodNode.WithModifiers(newModifiers).WithAdditionalAnnotations(Simplifier.Annotation);
				return newPatchMethodNode;
			}

			var newReturnType = AddXmlDocCommentToReturnType(patchMethodNode.ReturnType, baseMethodDocCommentId, recursionDepth: 0);

			if (newReturnType == null)
				return null;

			return patchMethodNode.WithReturnType(newReturnType);
		}

		private static TypeSyntax? AddXmlDocCommentToReturnType(TypeSyntax returnType, string baseMethodDocCommentId, int recursionDepth)
		{
			const int recursionMaxDepth = 100;

			if (recursionDepth > recursionMaxDepth)
				return null;

			switch (returnType)
			{
				case PredefinedTypeSyntax predefinedType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(predefinedType.Keyword, baseMethodDocCommentId);
						return tokenWithTrivia != null
							? predefinedType.WithKeyword(tokenWithTrivia.Value).WithAdditionalAnnotations(Simplifier.Annotation)
							: null;
					}
				case SimpleNameSyntax simpleOrGenericTypeName:
					{
						var newReturnType = AddXmlDocCommentToSimpleName(simpleOrGenericTypeName, baseMethodDocCommentId);
						return newReturnType;
					}
				case QualifiedNameSyntax qualifiedTypeName:
					{
						var newReturnType = AddXmlDocCommentToQualifiedName(qualifiedTypeName, baseMethodDocCommentId);
						return newReturnType;
					}

				case NullableTypeSyntax:
				case ArrayTypeSyntax:
				case PointerTypeSyntax:
					var elementType = GetElementType(returnType);

					if (elementType == null)
						return null;

					var newElementType = AddXmlDocCommentToReturnType(elementType, baseMethodDocCommentId,
																	  recursionDepth: recursionDepth + 1);
					if (newElementType == null)
						return null;

					return WithElementType(returnType, elementType);
				case TupleTypeSyntax tupleType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(tupleType.OpenParenToken, baseMethodDocCommentId);
						return tokenWithTrivia != null
							? tupleType.WithOpenParenToken(tokenWithTrivia.Value).WithAdditionalAnnotations(Simplifier.Annotation)
							: null;
					}
				case RefTypeSyntax refType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(refType.RefKeyword, baseMethodDocCommentId);
						return tokenWithTrivia != null
							? refType.WithRefKeyword(tokenWithTrivia.Value).WithAdditionalAnnotations(Simplifier.Annotation)
							: null;
					}
				case FunctionPointerTypeSyntax functionPointerType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(functionPointerType.DelegateKeyword, baseMethodDocCommentId);
						return tokenWithTrivia != null
							? functionPointerType.WithDelegateKeyword(tokenWithTrivia.Value).WithAdditionalAnnotations(Simplifier.Annotation)
							: null;
					}
				default:
					return null;
			}
		}

		private static QualifiedNameSyntax? AddXmlDocCommentToQualifiedName(QualifiedNameSyntax qualifiedTypeName, string baseMethodDocCommentId)
		{
			var prefixQualifiedNameSyntaxes = new Stack<QualifiedNameSyntax>(capacity: 4);
			prefixQualifiedNameSyntaxes.Push(qualifiedTypeName);

			NameSyntax? currentLeftPart = qualifiedTypeName.Left;
			NameSyntax? newMostLeftPart = null;

			while (currentLeftPart != null)
			{
				switch (currentLeftPart)
				{
					case QualifiedNameSyntax qualifiedNameLeftPart:
						prefixQualifiedNameSyntaxes.Push(qualifiedNameLeftPart);
						currentLeftPart = qualifiedNameLeftPart.Left;
						continue;

					case SimpleNameSyntax identifierNameSyntax:
						currentLeftPart = null;
						newMostLeftPart = AddXmlDocCommentToSimpleName(identifierNameSyntax, baseMethodDocCommentId);
						continue;

					case AliasQualifiedNameSyntax aliasQualifiedName:
						currentLeftPart = null;
						var newAliasNode = AddXmlDocCommentToSimpleName(aliasQualifiedName.Alias, baseMethodDocCommentId) as IdentifierNameSyntax;
						newMostLeftPart = newAliasNode != null
							? aliasQualifiedName.WithAlias(newAliasNode).WithAdditionalAnnotations(Simplifier.Annotation)
							: null;
						continue;

					default:
						break;
				}
			}

			if (newMostLeftPart == null)
				return null;

			NameSyntax currentMostLeftNewQualifiedName = newMostLeftPart;
			QualifiedNameSyntax? newReturnType = null;

			while (prefixQualifiedNameSyntaxes.Count > 0)
			{
				var currentMostLeftOldNode = prefixQualifiedNameSyntaxes.Pop();
				newReturnType = currentMostLeftOldNode.WithLeft(currentMostLeftNewQualifiedName).WithAdditionalAnnotations(Simplifier.Annotation);
				currentMostLeftNewQualifiedName = newReturnType;
			}

			return newReturnType;
		}

		private static SimpleNameSyntax? AddXmlDocCommentToSimpleName(SimpleNameSyntax simpleOrGenericTypeName, string baseMethodDocCommentId)
		{
			var tokenWithTrivia = AddXmlDocCommentToToken(simpleOrGenericTypeName.Identifier, baseMethodDocCommentId);
			return tokenWithTrivia != null
				? simpleOrGenericTypeName.WithIdentifier(tokenWithTrivia.Value).WithAdditionalAnnotations(Simplifier.Annotation)
				: null;
		}

		private static TypeSyntax? GetElementType(TypeSyntax? typeNode)
		{
			return typeNode switch
			{
				ArrayTypeSyntax arrayType 		=> arrayType.ElementType,
				NullableTypeSyntax nullableType => nullableType.ElementType,
				PointerTypeSyntax pointerType 	=> pointerType.ElementType,
				_ 								=> null
			};
		}

		private static TypeSyntax? WithElementType(TypeSyntax? typeNode, TypeSyntax newElementType)
		{
			return typeNode switch
			{
				ArrayTypeSyntax arrayType 		=> arrayType.WithElementType(newElementType),
				NullableTypeSyntax nullableType => nullableType.WithElementType(newElementType),
				PointerTypeSyntax pointerType 	=> pointerType.WithElementType(newElementType),
				_ 								=> null
			};
		}

		private static SyntaxToken? AddXmlDocCommentToToken(SyntaxToken tokenToAddTrivia, string baseMethodDocCommentId)
		{
			string comment = $"/// {XmlDocCommentOverridesPrefix} <seealso cref=\"{baseMethodDocCommentId}\"/>";
			var triviaList = ParseLeadingTrivia(comment);
			var commentTrivia = triviaList.FirstOrDefault(triviaList => triviaList.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

			if (commentTrivia == default)
				return null;

			if (commentTrivia.HasStructure && commentTrivia.GetStructure() is DocumentationCommentTriviaSyntax xmlDocCommentNode)
			{
				var crefNode = xmlDocCommentNode.DescendantNodes()
												.OfType<CrefSyntax>()
												.FirstOrDefault();
				if (crefNode != null)
				{
					crefNode = crefNode.WithAdditionalAnnotations(Simplifier.Annotation);
					xmlDocCommentNode = xmlDocCommentNode.ReplaceNode(crefNode, crefNode);
				}

				xmlDocCommentNode = xmlDocCommentNode.WithAdditionalAnnotations(Simplifier.Annotation);
				commentTrivia = Trivia(xmlDocCommentNode).WithAdditionalAnnotations(Simplifier.Annotation);
			}

			var oldLeadingTrivia = tokenToAddTrivia.LeadingTrivia;
			int? indexOfExistingTriviaWithCommentOrDirective = FindIndexOfLastTriviaWithCommentOrPreprocessorDirective(oldLeadingTrivia);
			IEnumerable<SyntaxTrivia> newTrivias;

			if (indexOfExistingTriviaWithCommentOrDirective != null)
			{
				int existingCommentTriviasCount = indexOfExistingTriviaWithCommentOrDirective.Value + 1;
				newTrivias = InsertNewXmlCommentAfterExistingCommentsAndDirectives(commentTrivia, existingCommentTriviasCount, oldLeadingTrivia);
			}
			else
			{
				newTrivias = InsertNewXmlCommentWhenNoCommentsAndDirectivesExisted(commentTrivia, oldLeadingTrivia);
			}

			var newLeadingTrivia = TriviaList(newTrivias);
			var newToken = tokenToAddTrivia.WithLeadingTrivia(newLeadingTrivia)
										   .WithAdditionalAnnotations(Simplifier.Annotation);
			return newToken;
		}

		private static int? FindIndexOfLastTriviaWithCommentOrPreprocessorDirective(in SyntaxTriviaList triviaList)
		{
			for (int i = triviaList.Count - 1; i >= 0; i--)
			{
				SyntaxTrivia trivia = triviaList[i];

				if (trivia.IsDirective || trivia.HasStructure || trivia.IsCommentTrivia())
					return i;
			}

			return null;
		}

		private static IEnumerable<SyntaxTrivia> InsertNewXmlCommentAfterExistingCommentsAndDirectives(in SyntaxTrivia commentTrivia,
																										int countOfExistingCommentTrivia,
																										in SyntaxTriviaList oldLeadingTrivia)
		{
			// for existing comments or preprocessor directives we add new comment after them
			var existingCommentsAndDirectives = oldLeadingTrivia.Take(countOfExistingCommentTrivia);
			var remainingExistingTrivias = oldLeadingTrivia.Skip(countOfExistingCommentTrivia);

			if (remainingExistingTrivias.Count == 0)
			{
				var newTrivias = existingCommentsAndDirectives.AppendItem(commentTrivia);
				return newTrivias;
			}
			else
			{
				var whiteSpaceIndentationTrivia = remainingExistingTrivias.Reverse()
																		  .TakeWhile((in SyntaxTrivia t) => !t.IsKind(SyntaxKind.EndOfLineTrivia))
																		  .Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
																		  .Reverse();

				var newTrivias = existingCommentsAndDirectives.Concat(whiteSpaceIndentationTrivia)
															  .AppendItem(commentTrivia);

				if (!remainingExistingTrivias.Any(SyntaxKind.EndOfLineTrivia))
					newTrivias = newTrivias.AppendItem(EndOfLine(Environment.NewLine));

				newTrivias = newTrivias.Concat(remainingExistingTrivias);
				return newTrivias;
			}
		}

		private static IEnumerable<SyntaxTrivia> InsertNewXmlCommentWhenNoCommentsAndDirectivesExisted(in SyntaxTrivia commentTrivia, 
																									   in SyntaxTriviaList oldLeadingTrivia)
		{
			var whiteSpaceIndentationTrivia = oldLeadingTrivia.Reverse()
															  .TakeWhile((in SyntaxTrivia t) => !t.IsKind(SyntaxKind.EndOfLineTrivia))
															  .Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
															  .Reverse();

			var newTrivias = whiteSpaceIndentationTrivia.PrependItem(EndOfLine(Environment.NewLine))
														.AppendItem(commentTrivia);

			if (!oldLeadingTrivia.Any(SyntaxKind.EndOfLineTrivia))
				newTrivias = newTrivias.AppendItem(EndOfLine(Environment.NewLine));

			newTrivias = newTrivias.Concat(oldLeadingTrivia);
			return newTrivias;
		}
	}
}
