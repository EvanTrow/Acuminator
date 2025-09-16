using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac;

public static class DacFieldKindExtensions
{
	private static readonly Dictionary<string, DacFieldKind> _systemDacFieldNamesToKinds = new(StringComparer.OrdinalIgnoreCase)
	{
		{ DacFieldNames.System.NoteID,	   DacFieldKind.NoteID },
		{ DacFieldNames.System.Timestamp,  DacFieldKind.Tstamp },
		{ DacFieldNames.System.GroupMask,  DacFieldKind.GroupMask },
		{ DacFieldNames.System.Attributes, DacFieldKind.Attributes },

		{ DacFieldNames.System.CreatedByID,			   DacFieldKind.CreatedByID },
		{ DacFieldNames.System.CreatedByScreenID,	   DacFieldKind.CreatedByScreenID },
		{ DacFieldNames.System.CreatedDateTime,		   DacFieldKind.CreatedDateTime },
		{ DacFieldNames.System.LastModifiedByID,	   DacFieldKind.LastModifiedByID },
		{ DacFieldNames.System.LastModifiedByScreenID, DacFieldKind.LastModifiedByScreenID },
		{ DacFieldNames.System.LastModifiedDateTime,   DacFieldKind.LastModifiedDateTime },

		{ DacFieldNames.Restricted.DeletedDatabaseRecord, DacFieldKind.DeletedDatabaseRecord },
		{ DacFieldNames.Restricted.DatabaseRecordStatus,  DacFieldKind.DatabaseRecordStatus },
		{ DacFieldNames.Restricted.CompanyID,			  DacFieldKind.CompanyID },
		{ DacFieldNames.Restricted.CompanyMask,			  DacFieldKind.CompanyMask },
		{ DacFieldNames.Restricted.Notes,				  DacFieldKind.Notes },
		{ DacFieldNames.Restricted.Files,				  DacFieldKind.Files }
	};


	/// <summary>
	/// Gets DAC field kind from the <paramref name="dacFieldName"/>.
	/// </summary>
	/// <param name="dacFieldName">Name of the DAC field.</param>
	/// <returns>
	/// The DAC field kind.
	/// </returns>
	public static DacFieldKind GetDacFieldKind(string dacFieldName) =>
		_systemDacFieldNamesToKinds.TryGetValue(dacFieldName.CheckIfNullOrWhiteSpace(), out DacFieldKind kind) 
			? kind 
			: DacFieldKind.Regular;

	/// <summary>
	/// Check if this is a system DAC field.
	/// </summary>
	/// <param name="kind">The DAC field kind to act on.</param>
	/// <returns>
	/// True if the DAC field is a system field, false if not.
	/// </returns>
	public static bool IsSystemField(this DacFieldKind kind) => 
		kind != DacFieldKind.Regular;

	/// <summary>
	/// Check if this is an audit DAC field.
	/// </summary>
	/// <param name="kind">The DAC field kind to act on.</param>
	/// <returns>
	/// True if the DAC field is an audit field, false if not.
	/// </returns>
	public static bool IsAuditField(this DacFieldKind kind) =>
		kind >= DacFieldKind.CreatedByID && kind <= DacFieldKind.LastModifiedDateTime;

	/// <summary>
	/// Check if this DAC field is reserved by runtime and forbidden in the application code.
	/// </summary>
	/// <param name="kind">The DAC field kind to act on.</param>
	/// <returns>
	/// True if the DAC field is reserved by runtime, false if not.
	/// </returns>
	public static bool IsReservedByRuntime(this DacFieldKind kind) =>
		kind >= DacFieldKind.DeletedDatabaseRecord && kind <= DacFieldKind.Files;
}
