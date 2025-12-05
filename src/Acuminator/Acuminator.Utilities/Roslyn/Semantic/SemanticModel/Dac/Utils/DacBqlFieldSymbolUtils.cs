using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac;

public static partial class DacPropertyAndFieldSymbolUtils
{
	/// <summary>
	/// Get corresponding DAC BQL field for a DAC field property <paramref name="property"/>.
	/// </summary>
	/// <param name="property">The property to act on. Must be a dac property.</param>
	/// <param name="pxContext">Acumatica Context.</param>
	/// <param name="checkContainingTypeIsDac">True for extra safety check that containing type is DAC or DAC extension.</param>
	/// <returns>
	/// The corresponding BQL field or null.
	/// </returns>
	public static INamedTypeSymbol? GetCorrespondingBqlField(this IPropertySymbol property, PXContext pxContext, bool checkContainingTypeIsDac)
	{
		property.ThrowOnNull();
		pxContext.ThrowOnNull();

		var containingDacOrDacExt = property.ContainingType;

		if (containingDacOrDacExt == null ||
			(checkContainingTypeIsDac && !containingDacOrDacExt.IsDacOrExtension(pxContext)))
		{
			return null;
		}

		var mappedBqlField = GetBqlFieldFromTypeByName(containingDacOrDacExt, property.Name);

		if (mappedBqlField != null)
			return mappedBqlField;

		var currentType = containingDacOrDacExt.BaseType;

		while (currentType != null && currentType.IsDAC(pxContext))
		{
			mappedBqlField = GetBqlFieldFromTypeByName(currentType, property.Name);

			if (mappedBqlField != null)
				return mappedBqlField;

			currentType = currentType.BaseType;
		}

		return null;
	}

	private static INamedTypeSymbol? GetBqlFieldFromTypeByName(INamedTypeSymbol type, string caseInsensitiveName)
	{
		var members = type.GetMembers();

		if (members.IsDefaultOrEmpty)
			return null;

		return members.OfType<INamedTypeSymbol>()
					  .FirstOrDefault(bqlField => caseInsensitiveName.Equals(bqlField.Name, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Gets the DAC BQL field data type from BQL field symbol.
	/// </summary>
	/// <param name="bqlField">The BQL field symbol.</param>
	/// <returns>
	/// The BQL field data type (like <see cref="string"/>) from BQL field symbol.
	/// </returns>
	public static ITypeSymbol? GetBqlFieldDataTypeFromBqlFieldSymbol(this ITypeSymbol bqlField)
	{
		bqlField.ThrowOnNull();

		if (bqlField.BaseType == null || bqlField.BaseType.Name != TypeNames.BqlField.Field)
			return null;

		var bqlFieldBqlDataType = bqlField.BaseType.ContainingType;		// this symbol represents types like BqlString, BqlInt, etc.

		if (bqlFieldBqlDataType?.Name != TypeNames.BqlField.BqlType || !bqlFieldBqlDataType.IsGenericType)
			return null;

		var bqlFieldTypeGenericTypeArgs = bqlFieldBqlDataType.TypeArguments;

		if (bqlFieldTypeGenericTypeArgs.Length != 2)
			return null;

		var bqlFieldDataType = bqlFieldTypeGenericTypeArgs[1];
		return bqlFieldDataType;
	}
}
