using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for BQL Field generation.
	/// </summary>
	/// <remarks>
	/// TODO After updated of Roslyn to 4.x.x to use C# language version to generate BQL fields without braces.
	/// </remarks>
	public static class BqlFieldGeneration
	{
		public static ClassDeclarationSyntax? GenerateWeaklyTypedBqlField(BqlFieldGenerationOptions generationOptions)
		{
			var iBqlFieldBaseTypeNode = IBqlFieldBaseTypeForBqlField();
			var bqlField = GenerateBqlField(generationOptions, iBqlFieldBaseTypeNode);

			return bqlField;
		}

		public static SimpleBaseTypeSyntax IBqlFieldBaseTypeForBqlField()
		{
			var iBqlFieldBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Data")),
							IdentifierName(TypeNames.BqlField.IBqlField)));
			return iBqlFieldBaseType;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(DataTypeName dataTypeName, BqlFieldGenerationOptions generationOptions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(dataTypeName, generationOptions.BqlFieldName,
														   generationOptions.BaseTypeNamingStyle);
			if (bqlFieldBaseTypeNode == null)
				return null;

			var bqlField = GenerateBqlField(generationOptions, bqlFieldBaseTypeNode);
			return bqlField;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(BqlFieldTypeName bqlFieldTypeName, BqlFieldGenerationOptions generationOptions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(bqlFieldTypeName, generationOptions.BqlFieldName,
														   generationOptions.BaseTypeNamingStyle);
			var bqlField = GenerateBqlField(generationOptions, bqlFieldBaseTypeNode);

			return bqlField;
		}

		public static SimpleBaseTypeSyntax? BaseTypeForBqlField(DataTypeName dataTypeName, string bqlFieldName,
																BqlFieldBaseTypeNamingStyle baseTypeNamingStyle)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldTypeName = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(dataTypeName).NullIfWhiteSpace();

			if (bqlFieldTypeName == null)
				return null;

			var bqlFieldType = BaseTypeForBqlFieldImpl(bqlFieldTypeName, bqlFieldName, baseTypeNamingStyle);
			return bqlFieldType;
		}

		public static SimpleBaseTypeSyntax BaseTypeForBqlField(BqlFieldTypeName bqlFieldTypeName, string bqlFieldName,
																BqlFieldBaseTypeNamingStyle baseTypeNamingStyle)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldType = BaseTypeForBqlFieldImpl(bqlFieldTypeName.Value, bqlFieldName, baseTypeNamingStyle);
			return bqlFieldType;
		}

		private static SimpleBaseTypeSyntax BaseTypeForBqlFieldImpl(string bqlFieldTypeName, string bqlFieldName,
																	BqlFieldBaseTypeNamingStyle baseTypeNamingStyle)
		{
			GenericNameSyntax fieldTypeNode =
				GenericName(Identifier("Field"))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList<TypeSyntax>(IdentifierName(bqlFieldName)))
						.WithGreaterThanToken(
							Token(leading: TriviaList(), SyntaxKind.GreaterThanToken, TriviaList(Space))));

			var bqlFieldNamespaceName = GetBqlFieldNamespaceName(bqlFieldTypeName, bqlFieldName, baseTypeNamingStyle);
			SimpleBaseTypeSyntax newBaseType;

			if (bqlFieldNamespaceName != null)
			{
				newBaseType =
					SimpleBaseType(
					QualifiedName(
						QualifiedName(
							bqlFieldNamespaceName,
							IdentifierName(bqlFieldTypeName)),
						fieldTypeNode));
			}
			else
			{
				newBaseType =
					SimpleBaseType(
						QualifiedName(
							IdentifierName(bqlFieldTypeName),
							fieldTypeNode));
			}

			return newBaseType;
		}

		private static QualifiedNameSyntax? GetBqlFieldNamespaceName(string bqlFieldTypeName, string bqlFieldName,
																	BqlFieldBaseTypeNamingStyle baseTypeNamingStyle)
		{
			bool isAttributesBqlField = bqlFieldName.Equals(DacFieldNames.System.Attributes, StringComparison.OrdinalIgnoreCase) &&
										bqlFieldTypeName.Equals(TypeNames.BqlField.BqlAttributes, StringComparison.OrdinalIgnoreCase);

			if (isAttributesBqlField)
			{
				var bqlFieldNamespaceName =
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Objects")),
							IdentifierName("CR"));

				return bqlFieldNamespaceName;
			}
			else if (baseTypeNamingStyle == BqlFieldBaseTypeNamingStyle.FullNameWithNamespace)
			{
				var bqlFieldNamespaceName =
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Data")),
							IdentifierName("BQL"));

				return bqlFieldNamespaceName;
			}
			else
				return null;
		}

		private static ClassDeclarationSyntax GenerateBqlField(BqlFieldGenerationOptions generationOptions, 
																SimpleBaseTypeSyntax bqlFieldBaseType)
		{
			var baseTypesListNode = BaseList(
										SingletonSeparatedList<BaseTypeSyntax>(bqlFieldBaseType));
			SyntaxTokenList modifiers = generationOptions.IsRedeclarationOfBaseField
				? TokenList(
					Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword), Token(SyntaxKind.AbstractKeyword))
				: TokenList(
					Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword));

			var bqlFieldNode = ClassDeclaration(
									attributeLists: default,
									modifiers,
									Identifier(generationOptions.BqlFieldName), typeParameterList: null, baseTypesListNode,
									constraintClauses: default, members: default)
								.WithOpenBraceToken(
									Token(leading: TriviaList(), SyntaxKind.OpenBraceToken, TriviaList()));

			var closeBracketToken = generationOptions.IsFirstField
				? Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken,
						TriviaList(CarriageReturn, LineFeed, CarriageReturn, LineFeed))
				: Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken, TriviaList(CarriageReturn, LineFeed));

			bqlFieldNode = bqlFieldNode.WithCloseBraceToken(closeBracketToken);

			if (generationOptions.AdjacentMemberToCopyRegions != null)
				bqlFieldNode = CopyRegionsFromMember(bqlFieldNode, generationOptions.AdjacentMemberToCopyRegions);

			return bqlFieldNode;
		}

		private static ClassDeclarationSyntax CopyRegionsFromMember(ClassDeclarationSyntax bqlFieldNode, 
																	MemberDeclarationSyntax adjacentMemberToCopyRegions)
		{
			var leadingTrivia = adjacentMemberToCopyRegions.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return bqlFieldNode;

			var bqlFieldNodeWithCopiedRegions = CodeGeneration.CopyRegionsFromTrivia(bqlFieldNode, leadingTrivia,
													copyBeforeNode: true, insertCopiedRegionsAfterNodeTrivia: true);
			return bqlFieldNodeWithCopiedRegions;
		}
	}
}
