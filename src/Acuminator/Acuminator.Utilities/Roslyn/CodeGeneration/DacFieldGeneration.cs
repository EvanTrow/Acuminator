using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for DAC Field generation.
	/// </summary>
	public static class DacFieldGeneration
	{
		/// <summary>
		/// Generates a DAC field node.
		/// </summary>
		/// <param name="dacFieldName">Name of the generated DAC field.</param>
		/// <param name="fieldGenerationOptions">Options for controlling the DAC field generation.</param>
		/// <param name="propertyAttributeLists">The property attribute lists.</param>
		/// <returns>
		/// The DAC field.
		/// </returns>
		public static (PropertyDeclarationSyntax FieldProperty, ClassDeclarationSyntax BqlField)? GenerateDacField(
																						string? dacFieldName, 
																						DacFieldGenerationOptions fieldGenerationOptions,
																						IEnumerable<AttributeListSyntax>? propertyAttributeLists)
		{
			if (dacFieldName.IsNullOrWhiteSpace() || fieldGenerationOptions == null)
				return null;

			string bqlFieldName = GetBqlFieldName(dacFieldName);
			var bqlFieldNode = BqlFieldGeneration.GenerateTypedBqlField(fieldGenerationOptions.NonNullablePropertyTypeName, bqlFieldName, 
																		fieldGenerationOptions.IsFirstField, isRedeclaration: false,
																		adjacentMemberToCopyRegions: null);
			if (bqlFieldNode == null)
				return null;

			var propertyNode = GenerateDacFieldProperty(dacFieldName, fieldGenerationOptions, propertyAttributeLists);

			if (propertyNode == null)
				return null;

			string regionName = char.IsUpper(dacFieldName[0])
				? dacFieldName
				: bqlFieldName;
			bqlFieldNode = AddRegionDirectiveToBqlNode(bqlFieldNode, regionName);

			return (propertyNode, bqlFieldNode);
		}

		private static string GetBqlFieldName(string dataFieldName) =>
			char.IsUpper(dataFieldName[0])
				? dataFieldName.FirstCharToLower()
				: dataFieldName.ToPascalCase();

		private static ClassDeclarationSyntax AddRegionDirectiveToBqlNode(ClassDeclarationSyntax bqlFieldNode, string regionName)
		{	
			var regionStartDirectiveNode = RegionDirectiveTrivia(isActive: true)
											.WithEndOfDirectiveToken(
												Token
												(
													TriviaList(PreprocessingMessage(regionName)),
													SyntaxKind.EndOfDirectiveToken,
													TriviaList()
												));

			var regionStartTrivia 	 = Trivia(regionStartDirectiveNode);
			var firstBqlNodeModifier = bqlFieldNode.Modifiers[0];
			var newLeadingTrivia 	 = firstBqlNodeModifier.LeadingTrivia.PrependItem(regionStartTrivia);
			var newFirstModifier	 = firstBqlNodeModifier.WithLeadingTrivia(newLeadingTrivia);

			var newModifiers = bqlFieldNode.Modifiers.RemoveAt(0)
													 .Insert(0, newFirstModifier);
			var newBqlFieldNode = bqlFieldNode.WithModifiers(newModifiers);
			return newBqlFieldNode;
		}

		private static PropertyDeclarationSyntax? GenerateDacFieldProperty(string dacFieldName, DacFieldGenerationOptions fieldGenerationOptions,
																		  IEnumerable<AttributeListSyntax>? propertyAttributeLists)
		{
			var propertyTypeNode = GeneratePropertyTypeNode(fieldGenerationOptions.NonNullablePropertyTypeName, 
															fieldGenerationOptions.IsNullablePropertyType);
			if (propertyTypeNode == null)
				return null;

			var propertyNode = PropertyDeclaration(propertyTypeNode, dacFieldName);
			var propertyModifiers = TokenList(Token(SyntaxKind.PublicKeyword));

			if (!fieldGenerationOptions.IsSealedDac)
			{
				propertyModifiers = propertyModifiers.Add(Token(SyntaxKind.VirtualKeyword));
			}

			propertyNode = propertyNode.WithModifiers(propertyModifiers);

			if (!propertyAttributeLists.IsNullOrEmpty())
			{
				propertyNode = propertyNode.WithAttributeLists(List(propertyAttributeLists));
			}

			if (propertyNode.AccessorList == null)
				return null;

			var closeBraceToken 			= propertyNode.AccessorList.CloseBraceToken;
			var endRegionTrivia 			= Trivia(EndRegionDirectiveTrivia(isActive: true));
			var closeBraceNewTrailingTrivia = closeBraceToken.TrailingTrivia.Add(endRegionTrivia);
			var newCloseBraceToken 			= closeBraceToken.WithTrailingTrivia(closeBraceNewTrailingTrivia);
			
			propertyNode = propertyNode.WithAccessorList(
											propertyNode.AccessorList.WithCloseBraceToken(newCloseBraceToken));
			return propertyNode;
		}

		private static TypeSyntax? GeneratePropertyTypeNode(DataTypeName nonNullablePropertyTypeName, bool isNullablePropertyType)
		{
			try
			{
				TypeSyntax parsedTypeName = SyntaxFactory.ParseTypeName(nonNullablePropertyTypeName.Value);
				return isNullablePropertyType
					? NullableType(parsedTypeName)
					: parsedTypeName;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
