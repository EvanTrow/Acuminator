#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Dac
{
	public enum DacMemberCategory
	{
		BaseTypes,
		Keys,
		AllDacFields,
		NonBqlProperties,
		InitializationAndActivation,
	}

	internal static class DacMemberTypeTypeUtils
	{
		private static readonly Dictionary<DacMemberCategory, string> _descriptions = new()
		{
			{ DacMemberCategory.BaseTypes, "Base Types" },
			{ DacMemberCategory.Keys, "Keys" },
			{ DacMemberCategory.AllDacFields, "DAC Fields" },
			{ DacMemberCategory.NonBqlProperties, "Non-BQL Properties" },
			{ DacMemberCategory.InitializationAndActivation, "Initialization & Activation" }
		};

		public static string Description(this DacMemberCategory dacMemberCategory) =>
			_descriptions.TryGetValue(dacMemberCategory, out string description)
				? description
				: string.Empty;
	}
}
