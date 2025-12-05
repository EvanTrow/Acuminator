using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;

/// <summary>
/// Interface for Acumatica Framework extension types.
/// </summary>
public interface IExtensionInfo<TExtension>
where TExtension : class, IExtensionInfo<TExtension>
{
	/// <summary>
	/// The type of the mechanism used to extend the base extensions of this extension.
	/// </summary>
	ExtensionMechanismType BaseExtensionsMechanismType { get; }

	ImmutableArray<TExtension> BaseExtensions { get; }
}