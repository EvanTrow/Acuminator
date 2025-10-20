using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac;

public static class DacFieldCategoryExtensions
{
	private static readonly Dictionary<string, DacFieldCategory> _systemDacFieldNamesToCategories = new(StringComparer.OrdinalIgnoreCase)
	{
		{ DacFieldNames.System.NoteID,	   DacFieldCategory.NoteID },
		{ DacFieldNames.System.Timestamp,  DacFieldCategory.tstamp },
		{ DacFieldNames.System.GroupMask,  DacFieldCategory.GroupMask },
		{ DacFieldNames.System.Attributes, DacFieldCategory.Attributes },

		{ DacFieldNames.System.CreatedByID,			   DacFieldCategory.CreatedByID },
		{ DacFieldNames.System.CreatedByScreenID,	   DacFieldCategory.CreatedByScreenID },
		{ DacFieldNames.System.CreatedDateTime,		   DacFieldCategory.CreatedDateTime },
		{ DacFieldNames.System.LastModifiedByID,	   DacFieldCategory.LastModifiedByID },
		{ DacFieldNames.System.LastModifiedByScreenID, DacFieldCategory.LastModifiedByScreenID },
		{ DacFieldNames.System.LastModifiedDateTime,   DacFieldCategory.LastModifiedDateTime },

		{ DacFieldNames.Restricted.DeletedDatabaseRecord, DacFieldCategory.DeletedDatabaseRecord },
		{ DacFieldNames.Restricted.DatabaseRecordStatus,  DacFieldCategory.DatabaseRecordStatus },
		{ DacFieldNames.Restricted.CompanyID,			  DacFieldCategory.CompanyID },
		{ DacFieldNames.Restricted.CompanyMask,			  DacFieldCategory.CompanyMask },
		{ DacFieldNames.Restricted.Notes,				  DacFieldCategory.Notes },
		{ DacFieldNames.Restricted.Files,				  DacFieldCategory.Files }
	};


	/// <summary>
	/// Gets DAC field category from the <paramref name="dacFieldName"/>.
	/// </summary>
	/// <param name="dacFieldName">Name of the DAC field.</param>
	/// <returns>
	/// The DAC field category.
	/// </returns>
	public static DacFieldCategory GetDacFieldCategory(string dacFieldName) =>
		_systemDacFieldNamesToCategories.TryGetValue(dacFieldName.CheckIfNullOrWhiteSpace(), out DacFieldCategory category) 
			? category
			: DacFieldCategory.Regular;

	/// <summary>
	/// Check if this is a system DAC field.
	/// </summary>
	/// <param name="category">The DAC field category to act on.</param>
	/// <returns>
	/// True if the DAC field is a system field, false if not.
	/// </returns>
	public static bool IsSystemField(this DacFieldCategory category) => 
		category != DacFieldCategory.Regular;

	/// <summary>
	/// Check if this is an audit DAC field.
	/// </summary>
	/// <param name="kind">The DAC field kind to act on.</param>
	/// <returns>
	/// True if the DAC field is an audit field, false if not.
	/// </returns>
	public static bool IsAuditField(this DacFieldCategory category) =>
		category >= DacFieldCategory.CreatedByID && category <= DacFieldCategory.LastModifiedDateTime;


	/// <summary>
	/// Check if this is Created audit DAC field.
	/// </summary>
	/// <param name="category">The DAC field category to act on.</param>
	/// <returns>
	/// True if the DAC field is Created audit field, false if not.
	/// </returns>
	public static bool IsCreatedAuditField(this DacFieldCategory category) =>
		category >= DacFieldCategory.CreatedByID && category <= DacFieldCategory.CreatedDateTime;

	/// <summary>
	/// Check if this is LastModified audit DAC field.
	/// </summary>
	/// <param name="category">The DAC field category to act on.</param>
	/// <returns>
	/// True if the DAC field is LastModified audit field, false if not.
	/// </returns>
	public static bool IsLastModifiedAuditField(this DacFieldCategory category) =>
		category >= DacFieldCategory.LastModifiedByID && category <= DacFieldCategory.LastModifiedDateTime;

	/// <summary>
	/// Check if this DAC field is reserved by runtime and forbidden in the application code.
	/// </summary>
	/// <param name="category">The DAC field category to act on.</param>
	/// <returns>
	/// True if the DAC field is reserved by runtime, false if not.
	/// </returns>
	public static bool IsReservedByRuntime(this DacFieldCategory category) =>
		category >= DacFieldCategory.DeletedDatabaseRecord && category <= DacFieldCategory.Files;
}
