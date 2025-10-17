using System;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

internal static class DiagnosticProperties
{
	/// <summary>
	/// Property stores infos about missing mandatory DAC fields separated by <see cref="Constants.FieldKindsSeparator"/>.<br/>
	/// Each info consists of a pair of <see cref="DacFieldKind"/> and <see cref="DacFieldInsertMode"/> values separated by 
	/// <see cref="Constants.FieldKindAndInsertModeSeparator"/>.
	/// </summary>
	public const string MissingMandatoryDacFieldsInfos = nameof(MissingMandatoryDacFieldsInfos);

	/// <summary>
	/// Flag indicating whether the DAC is sealed.
	/// </summary>
	public const string IsSealedDac = nameof(IsSealedDac);
}