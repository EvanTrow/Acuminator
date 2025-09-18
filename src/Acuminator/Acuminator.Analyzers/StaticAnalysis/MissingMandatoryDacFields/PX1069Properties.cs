using System;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

internal static class PX1069Properties
{
	/// <summary>
	/// Property stores comma-separated <see cref="DacFieldKind"/> values that represent mandatory DAC fields missing in the DAC.
	/// </summary>
	public const string MissingMandatoryDacFields = nameof(MissingMandatoryDacFields);
}