using System;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

/// <summary>
/// DAC field insert modes.
/// </summary>
internal enum DacFieldInsertMode : byte
{
	/// <summary>
	/// Insert DAC field at the beginning of the DAC.
	/// </summary>
	AtTheBeginning,

	/// <summary>
	/// Insert DAC field at the end of the DAC.
	/// </summary>
	AtTheEnd,

	/// <summary>
	/// Insert DAC field before the first Created audit field (CreatedByID, CreatedByScreenID, CreatedDateTime).
	/// </summary>
	BeforeFirstCreatedAuditField,

	/// <summary>
	/// Insert DAC field after the last Created audit field (CreatedByID, CreatedByScreenID, CreatedDateTime).
	/// </summary>
	AfterLastCreatedAuditField,

	/// <summary>
	/// Insert DAC field before the first LastModified audit field (LastModifiedID, LastModifiedScreenID, LastModifiedDateTime).
	/// </summary>
	BeforeFirstLastModifiedAuditField,

	/// <summary>
	/// Insert DAC field after the last LastModified audit field (LastModifiedID, LastModifiedScreenID, LastModifiedDateTime).
	/// </summary>
	AfterLastLastModifiedAuditField
}