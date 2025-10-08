using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// A DAC field generation options.
	/// </summary>
	/// <param name="FieldProperty">Property node for the DAC field property.</param>
	/// <param name="BqlField">Class node for the DAC BQL field.</param>
	public record struct GeneratedDacFieldNodeInfo(PropertyDeclarationSyntax FieldProperty, ClassDeclarationSyntax BqlField);
}
