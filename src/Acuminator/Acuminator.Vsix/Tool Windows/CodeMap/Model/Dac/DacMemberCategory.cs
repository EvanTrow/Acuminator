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
		AuditDacFields,
		SystemNonAuditDacFields,
		NonBqlProperties,
		InitializationAndActivation,
	}

	internal static class DacMemberTypeTypeUtils
	{
		private static readonly Dictionary<DacMemberCategory, string> _descriptions = new()
		{
			{ DacMemberCategory.BaseTypes, 					 VSIXResource.CodeMap_DAC_MemberCategories_BaseTypes },
			{ DacMemberCategory.Keys, 						 VSIXResource.CodeMap_DAC_MemberCategories_Keys },
			{ DacMemberCategory.AllDacFields, 				 VSIXResource.CodeMap_DAC_MemberCategories_AllFields },
			{ DacMemberCategory.AuditDacFields, 			 VSIXResource.CodeMap_DAC_MemberCategories_AuditFields },
			{ DacMemberCategory.SystemNonAuditDacFields, 	 VSIXResource.CodeMap_DAC_MemberCategories_SystemNonAuditFields },
			{ DacMemberCategory.NonBqlProperties, 			 VSIXResource.CodeMap_DAC_MemberCategories_NonBQLProperties },
			{ DacMemberCategory.InitializationAndActivation, VSIXResource.CodeMap_DAC_MemberCategories_InitializationAndActivation }
		};

		public static string Description(this DacMemberCategory dacMemberCategory) =>
			_descriptions.TryGetValue(dacMemberCategory, out string description)
				? description
				: string.Empty;
	}
}
