using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public partial class AddXmlDocCommentWithReferenceToBaseMethodFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.Id
			);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			string title = nameof(Resources.PX1098Fix).GetLocalized().ToString();
			var document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cToken => AddXmlDocCommentToPatchMethod(document, context.Span, cToken),
											   equivalenceKey: nameof(Resources.PX1097Fix));
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static async Task<Document> AddXmlDocCommentToPatchMethod(Document document, TextSpan span, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var patchMethodNode = root?.FindNode(span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (patchMethodNode == null)
				return document;

			var newPatchMethodNode = AddXmlDocCommentToPatchMethod(patchMethodNode);

			if (newPatchMethodNode == null || ReferenceEquals(patchMethodNode, newPatchMethodNode))
				return document;

			var newRoot = root!.ReplaceNode(patchMethodNode, newPatchMethodNode);
			return document.WithSyntaxRoot(newRoot);
		}

		private static MethodDeclarationSyntax? AddXmlDocCommentToPatchMethod(MethodDeclarationSyntax patchMethodNode)
		{
			if (patchMethodNode.AttributeLists.Count > 0)
			{
				var firstAttributeList = patchMethodNode.AttributeLists[0];
				var tokenWithTrivia = AddXmlDocCommentToToken(firstAttributeList.OpenBracketToken);

				if (tokenWithTrivia == null)
					return null;

				var newAttributeLists =
					patchMethodNode.AttributeLists.RemoveAt(0)
												  .Insert(0, firstAttributeList.WithOpenBracketToken(tokenWithTrivia.Value));
				var newPatchMethodNode = patchMethodNode.WithAttributeLists(newAttributeLists);
				return newPatchMethodNode;
			}

			var modifiers = patchMethodNode.Modifiers;

			if (modifiers.Count > 0)
			{
				var firstModifier = modifiers[0];
				var tokenWithTrivia = AddXmlDocCommentToToken(firstModifier);

				if (tokenWithTrivia == null)
					return null;

				var newModifiers = modifiers.RemoveAt(0)
											.Insert(0, tokenWithTrivia.Value);
				var newPatchMethodNode = patchMethodNode.WithModifiers(newModifiers);
				return newPatchMethodNode;
			}

			var newReturnType = AddXmlDocCommentToReturnType(patchMethodNode.ReturnType, recursionDepth: 0);

			if (newReturnType == null)
				return null;

			return patchMethodNode.WithReturnType(newReturnType);
		}

		private static TypeSyntax? AddXmlDocCommentToReturnType(TypeSyntax returnType, int recursionDepth)
		{
			const int recursionMaxDepth = 100;

			if (recursionDepth > recursionMaxDepth)
				return null; 

			switch (returnType)
			{
				case PredefinedTypeSyntax predefinedType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(predefinedType.Keyword);
						return tokenWithTrivia != null
							? predefinedType.WithKeyword(tokenWithTrivia.Value)
							: null;
					}
				case SimpleNameSyntax simpleOrGenericTypeName:
					{
						var newReturnType = AddXmlDocCommentToSimpleName(simpleOrGenericTypeName);
						return newReturnType;
					}
				case QualifiedNameSyntax qualifiedTypeName:
					{
						var newReturnType = AddXmlDocCommentToQualifiedName(qualifiedTypeName);
						return newReturnType;
					}

				case NullableTypeSyntax:
				case ArrayTypeSyntax:
				case PointerTypeSyntax:
					var elementType = GetElementType(returnType);

					if (elementType == null)
						return null;

					var newElementType = AddXmlDocCommentToReturnType(elementType, recursionDepth: recursionDepth + 1);
					if (newElementType == null)
						return null;

					return WithElementType(returnType, elementType);
				case TupleTypeSyntax tupleType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(tupleType.OpenParenToken);
						return tokenWithTrivia != null
							? tupleType.WithOpenParenToken(tokenWithTrivia.Value)
							: null;
					}
				case RefTypeSyntax refType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(refType.RefKeyword);
						return tokenWithTrivia != null
							? refType.WithRefKeyword(tokenWithTrivia.Value)
							: null;
					}
				case FunctionPointerTypeSyntax functionPointerType:
					{
						var tokenWithTrivia = AddXmlDocCommentToToken(functionPointerType.DelegateKeyword);
						return tokenWithTrivia != null
							? functionPointerType.WithDelegateKeyword(tokenWithTrivia.Value)
							: null;
					}
				default:
					return null;
			}
		}

		private static QualifiedNameSyntax? AddXmlDocCommentToQualifiedName(QualifiedNameSyntax qualifiedTypeName)
		{
			 

		
		}

		private static SimpleNameSyntax? AddXmlDocCommentToSimpleName(SimpleNameSyntax simpleOrGenericTypeName)
		{
			var tokenWithTrivia = AddXmlDocCommentToToken(simpleOrGenericTypeName.Identifier);
			return tokenWithTrivia != null
				? simpleOrGenericTypeName.WithIdentifier(tokenWithTrivia.Value)
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

		private static SyntaxToken? AddXmlDocCommentToToken(SyntaxToken tokenToAddTrivia)
		{
			
		}
	}
}
