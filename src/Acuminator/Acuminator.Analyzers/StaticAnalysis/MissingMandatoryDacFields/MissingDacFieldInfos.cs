using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

public readonly record struct MissingMandatoryDacFieldInfo(DacFieldKind FieldKind, DacFieldInsertMode InsertMode);

public readonly record struct MissingDacFieldsInfos(List<MissingMandatoryDacFieldInfo> MissingAuditAndTimestampInfos,
													MissingMandatoryDacFieldInfo? MissingNoteIdFieldInfo);