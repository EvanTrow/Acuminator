namespace Acuminator.Utilities.Roslyn.Semantic.Dac;

/// <summary>
/// Values that represent DAC field kinds.
/// </summary>
public enum DacFieldKind : byte
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
	Notes,
	Files
}
