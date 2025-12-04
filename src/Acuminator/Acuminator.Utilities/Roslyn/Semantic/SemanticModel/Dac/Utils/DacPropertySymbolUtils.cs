using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Shared;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public static partial class DacPropertyAndFieldSymbolUtils
	{
		public static DacFieldSize GetFieldSize(this DacPropertyInfo dacProperty, PXContext pxContext)
		{
			var foreignFieldSizes = dacProperty.CheckIfNull().DeclaredDataTypeAttributes
															 .AllDeclaredDatatypeAttributesOnDacProperty
															 .Select(dataTypeAttr => dataTypeAttr.GetFieldSize(pxContext));

			DacFieldSize consolidatedFieldSize = DacFieldSize.NotDefined;

			foreach (DacFieldSize fieldSize in foreignFieldSizes)
			{
				if (fieldSize.IsNotDefined)
					continue;
				else if (fieldSize.IsInconsistent)
					return DacFieldSize.MultipleSizesDeclared;			// Size is inconsistent

				if (consolidatedFieldSize.IsNotDefined)
					consolidatedFieldSize = fieldSize;
				else if (!consolidatedFieldSize.Equals(fieldSize))
					return DacFieldSize.MultipleSizesDeclared;			// Size is inconsistent
			}

			return consolidatedFieldSize;
		}
	}
}
