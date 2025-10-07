using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// A DAC field generation options.
	/// </summary>
	/// <param name="NonNullablePropertyTypeName">Name of the non nullable part of the property type.</param>
	/// <param name="IsNullablePropertyType">True if is nullable property type, false if not. Nullable here include nullable reference types</param>
	/// <param name="IsSealedDac">True if containing DAC is sealed, false if not.</param>
	/// <param name="IsFirstField">True if generated DAC field will be a first field in the DAC, false if not.</param>
	public record DacFieldGenerationOptions(DataTypeName NonNullablePropertyTypeName, bool IsNullablePropertyType, bool IsSealedDac,
											bool IsFirstField);
}
