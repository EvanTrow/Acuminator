using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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

		///// Overrides <seealso cref="WorksheetPicking.IsWorksheetMode(string)"/>
		//public bool IsWorksheetMode(string modeCode, Func<string, bool> base_IsWorksheetMode) => base_IsWorksheetMode(modeCode);

		private static SyntaxToken? AddXmlDocCommentToToken(SyntaxToken tokenToAddTrivia, string baseMethodDocCommentId)
		{
			////var n = SyntaxFactory.Comment(comment);
			//var xmlTextNodeWithOverridesPrefix = XmlText(
			//										XmlTextLiteral(XmlDocCommentOverridesPrefix));
			//var seeAlsoElement = CreateSeeAlsoElementWithReferenceToBaseMethod(patchMethodNode, );
			//var xmlTextSuffixWithNewLine = XmlText(
			//									XmlTextNewLine(Environment.NewLine, continueXmlDocumentationComment: false));
			//XmlNodeSyntax[] xmlDocComments =
			//[
			//	xmlTextNodeWithOverridesPrefix,
			//	seeAlsoElement,
			//	xmlTextSuffixWithNewLine
			//];

			//var documentationTrivia =
			//	DocumentationCommentTrivia(
			//		SyntaxKind.SingleLineDocumentationCommentTrivia,
			//		List(xmlDocComments))
			//	.WithEndOfComment(
			//		Token(SyntaxKind.EndOfDocumentationCommentToken));

			string comment = $"/// {XmlDocCommentOverridesPrefix} <seealso cref=\"{baseMethodDocCommentId}\"/>";
			SyntaxTrivia commentTrivia = Comment(comment).WithAdditionalAnnotations(Simplifier.Annotation);

			var whiteSpaceIndentationTrivia = tokenToAddTrivia.LeadingTrivia.Reverse()
																			.TakeWhile(t => !t.IsKind(SyntaxKind.EndOfLineTrivia))
																			.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
			var newTrivia = TriviaList(EndOfLine(Environment.NewLine))
								.AddRange(whiteSpaceIndentationTrivia)
								.Add(commentTrivia)
								.AddRange(tokenToAddTrivia.LeadingTrivia);
			var newToken = tokenToAddTrivia.WithLeadingTrivia(newTrivia)
										   .WithAdditionalAnnotations(Simplifier.Annotation);
			return newToken;
		}

		//private static XmlEmptyElementSyntax? CreateSeeAlsoElementWithReferenceToBaseMethod(MethodDeclarationSyntax patchMethodNode)
		//{
			



		//	XmlCrefAttribute(
		//		QualifiedCref(
		//			IdentifierName("WorksheetPicking"),
		//			NameMemberCref(
		//				IdentifierName("IsWorksheetMode"))
		//			.WithParameters(
		//				CrefParameterList(
		//					SingletonSeparatedList(
		//						CrefParameter(
		//							PredefinedType(
		//								Token(SyntaxKind.StringKeyword)))))
		//				.WithOpenParenToken(
		//					Token(SyntaxKind.OpenParenToken))
		//				.WithCloseParenToken(
		//					Token(SyntaxKind.CloseParenToken))))
		//		.WithDotToken(
		//			Token(SyntaxKind.DotToken)))
		//	.WithName(
		//		XmlName(
		//			Identifier(
		//				TriviaList(
		//					Space),
		//				"cref",
		//				TriviaList())))
		//	.WithEqualsToken(
		//		Token(SyntaxKind.EqualsToken))
		//	.WithStartQuoteToken(
		//		Token(SyntaxKind.DoubleQuoteToken))
		//	.WithEndQuoteToken(
		//		Token(SyntaxKind.DoubleQuoteToken))


		//	var seeAlsoElement = XmlSeeAlsoElement(cRefAttribute); XmlEmptyElement(
		//		   XmlName(
		//			   Identifier("seealso")))
		//	   .WithLessThanToken(
		//		   Token(SyntaxKind.LessThanToken))
		//	   .WithAttributes(
		//		   SingletonList<XmlAttributeSyntax>(

		//			   ))
		//	   .WithSlashGreaterThanToken(
		//		   Token(SyntaxKind.SlashGreaterThanToken));

		//	return seeAlsoElement;
		//}
	}
}
