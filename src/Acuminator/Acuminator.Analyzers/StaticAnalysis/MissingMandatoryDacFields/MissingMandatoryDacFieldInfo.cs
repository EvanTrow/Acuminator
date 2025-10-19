using System;
using System.Collections.Generic;

using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

public readonly record struct MissingMandatoryDacFieldInfo(DacFieldCategory FieldCategory, DacFieldInsertMode InsertMode);