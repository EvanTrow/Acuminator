namespace Acuminator.Utilities.Roslyn.Semantic.Dac;

/// <summary>
/// Values that represent existing DAC field categories.
/// </summary>
public enum DacFieldCategory : byte
{
	Regular,

	NoteID,
	tstamp,
	GroupMask,
	Attributes,

	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,

	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,

	DeletedDatabaseRecord,
	DatabaseRecordStatus,
	CompanyID,
	CompanyMask,
	CompanyPrefix,
	Notes,
	Files
}
