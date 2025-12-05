using System;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions
{
	/// <summary>
	/// The type of the mechanism used to extend base extension.
	/// </summary>
	public enum ExtensionMechanismType : byte
	{
		None,

		/// <summary>
		/// The C# inheritance.
		/// </summary>
		Inheritance,

		/// <summary>
		/// Acumatica Framework's extension chaining with a higher-level extension.
		/// </summary>
		Chaining
	}
}