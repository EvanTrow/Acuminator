using System;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Styles of base types names for BQL fields.
	/// </summary>
	public enum BqlFieldBaseTypeNamingStyle : byte
	{
		FullNameWithNamespace,
		OnlyTypeName
	}

	/// <summary>
	/// A BQL field generation options.
	/// </summary>
	/// <param name="BqlFieldName">Name of the BQL field.</param>
	/// <param name="IsFirstField">Flag indicating whether BQL field is added as a first field in the DAC.</param>
	/// <param name="IsRedeclarationOfBaseField">
	/// Flag indicating whether the BQL field is added as a redeclaration of a BQL field from the base DAC.<br/>
	/// In this case the 'new' modifier will be added to the generated field.
	/// </param>
	/// <param name="AdjacentMemberToCopyRegions">Adjacent member to copy regions from.</param>
	/// <param name="BaseTypeNamingStyle">Style of base type name for the BQL field.</param>
	/// <param name="CSharpVersion">(Optional) Effective C# language version.</param>
	public record BqlFieldGenerationOptions(string BqlFieldName, bool IsFirstField, bool IsRedeclarationOfBaseField, 
											MemberDeclarationSyntax? AdjacentMemberToCopyRegions,
											BqlFieldBaseTypeNamingStyle BaseTypeNamingStyle, 
											LanguageVersion? CSharpVersion = null);
}
