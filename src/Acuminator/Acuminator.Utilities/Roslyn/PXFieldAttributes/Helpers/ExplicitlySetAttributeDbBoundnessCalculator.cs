using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes;

internal static class ExplicitlySetAttributeDbBoundnessCalculator
{
	public static DbBoundnessType GetDbBoundnessSetExplicitlyByAttributeApplication(AttributeData attributeApplication)
	{
		if (attributeApplication.NamedArguments.IsDefaultOrEmpty)
			return DbBoundnessType.NotDefined;

		int isDbFieldCounter = 0, isNonDbCounter = 0;
		bool? isDbFieldValue = null, isNonDbValue = null;

		foreach (var (argumentName, argumentValue) in attributeApplication.NamedArguments)
		{
			if (string.Equals(argumentName, Constants.IsDBField, StringComparison.OrdinalIgnoreCase))
			{
				isDbFieldCounter++;

				if (argumentValue.Value is bool boolIsDbFieldValue)
					isDbFieldValue = boolIsDbFieldValue;
			}

			if (string.Equals(argumentName, Constants.NonDB, StringComparison.OrdinalIgnoreCase))
			{
				isNonDbCounter++;

				if (argumentValue.Value is bool boolNonDbValue)
					isNonDbValue = boolNonDbValue;
			}
		}

		if (isDbFieldCounter > 1 || isNonDbCounter > 1)
			return DbBoundnessType.Unknown;                 //Strange case when there are multiple different "IsDBField"/"NonDB" properties set in attribute constructor (with different letters register case)

		bool isDbFieldPresent = isDbFieldCounter == 1;
		bool isNonDbPresent = isNonDbCounter == 1;

		if (!isDbFieldPresent && !isNonDbPresent)
			return DbBoundnessType.NotDefined;
		else if (isDbFieldPresent && isNonDbPresent)        //Strange case when there are both IsDBField and NonDB properties
		{
			DbBoundnessType isDbFieldBoundness = GetIsDbFieldBoundness(isDbFieldValue);
			DbBoundnessType nonDbBoundness = GetNonDbBoundness(isNonDbValue);
			return isDbFieldBoundness.Combine(nonDbBoundness);
		}
		else if (isDbFieldPresent)
			return GetIsDbFieldBoundness(isDbFieldValue);
		else
			return GetNonDbBoundness(isNonDbValue);

		//------------------------------------Local function--------------------------------------------------------------------
		static DbBoundnessType GetIsDbFieldBoundness(bool? isDbFieldValue) =>
			!isDbFieldValue.HasValue
				? DbBoundnessType.Unknown           //Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
				: isDbFieldValue.Value
					? DbBoundnessType.DbBound
					: DbBoundnessType.Unbound;

		static DbBoundnessType GetNonDbBoundness(bool? isNonDbValue) =>
			!isNonDbValue.HasValue
				? DbBoundnessType.Unknown          //Strange rare case when NonDB property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
				: isNonDbValue.Value
					? DbBoundnessType.Unbound
					: DbBoundnessType.DbBound;
	}
}
