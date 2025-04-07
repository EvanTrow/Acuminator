using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// A helper which gets field size from the data type attribute.
	/// </summary>
	public static class FieldSizeGetter
	{
		public static DacFieldSize GetFieldSize(this DacFieldAttributeInfo dataTypeAttribute, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (dataTypeAttribute.CheckIfNull().DbBoundness is DbBoundnessType.NotDefined)
				return DacFieldSize.NotDefined;

			int? fieldSize = null;

			foreach (AttributeWithApplication aggregatedAttr in dataTypeAttribute.FlattenedAcumaticaAttributes)
			{
				int? sizeFromAggregatedAttribute = GetSizeFromAggregatedAttribute(aggregatedAttr.Application, pxContext);

				if (sizeFromAggregatedAttribute is null)
					continue;

				if (fieldSize is null)
				{
					fieldSize = sizeFromAggregatedAttribute;
				}
				else if (fieldSize != sizeFromAggregatedAttribute)    // found two inconsistent sizes
				{
					return DacFieldSize.MultipleSizesDeclared;
				}
			}

			return fieldSize.HasValue
				? new DacFieldSize(fieldSize.Value)
				: DacFieldSize.NotDefined;
		}

		private static int? GetSizeFromAggregatedAttribute(AttributeData aggregatedAttribute, PXContext pxContext)
		{
			if (aggregatedAttribute.AttributeClass is null)
				return null;

			var attributesWithHardCodedLength = pxContext.FieldAttributes.DataAttributesWithHardcodedLength;

			if (attributesWithHardCodedLength.TryGetValue(aggregatedAttribute.AttributeClass, out int hardCodedSize))
				return hardCodedSize;

			return GetLengthArgumentFromConstructor(aggregatedAttribute, pxContext);
		}

		private static int? GetLengthArgumentFromConstructor(AttributeData aggregatedAttribute, PXContext pxContext)
		{
			if (aggregatedAttribute.AttributeConstructor?.Parameters.Length is null or 0 ||
				!IsAttributeWithLengthProperty(aggregatedAttribute, pxContext))
			{
				return null;
			}

			// We can't do the regular mapping here, since Acuminator resolves mapping between arguments and parameters only for methods
			// Mapping of attribute's constructor arguments require special handling. So we'll first try direct matching by index
			var indexOfSizeParamFromAttrConstructor =
				aggregatedAttribute.AttributeConstructor.Parameters.FindIndex(parameter =>
							parameter.Type.SpecialType == SpecialType.System_Int32 &&
							(parameter.Name.Equals(PropertyNames.Attributes.Length, StringComparison.OrdinalIgnoreCase) ||
							 parameter.Name.Equals(PropertyNames.Attributes.Size, StringComparison.OrdinalIgnoreCase)));

			if (indexOfSizeParamFromAttrConstructor < 0)
				return null;

			// Try direct matching by index first to cover 99% of cases 
			if (indexOfSizeParamFromAttrConstructor < aggregatedAttribute.ConstructorArguments.Length)
			{
				var constructorArgAtIndex = aggregatedAttribute.ConstructorArguments[indexOfSizeParamFromAttrConstructor];
				int? lengthArgFromDirectMatching = constructorArgAtIndex.Value as int?;

				if (lengthArgFromDirectMatching.HasValue)
					return lengthArgFromDirectMatching;
			}

			// Try finding any integer constructor argument
			var integerConstructorArgs = aggregatedAttribute.ConstructorArguments
															.Where(arg => arg.Kind == TypedConstantKind.Primitive && arg.Value is int sizeValue &&
																		  sizeValue >= 0)	// heuristic - field size is probably higher than zero
															.ToList();
			if (integerConstructorArgs.Count == 1)
				return integerConstructorArgs[0].Value as int?;
			else if (integerConstructorArgs.Count > 1)
			{
				int? sizeFromMultipleArgs = null;

				foreach (TypedConstant constructorArg in integerConstructorArgs)
				{
					int sizeFromArg = (int)constructorArg.Value!;

					if (sizeFromMultipleArgs is null)
						sizeFromMultipleArgs = sizeFromArg;
					else if (sizeFromMultipleArgs != sizeFromArg)
						return null;                                    // found two inconsistent sizes, can't select which one to use as a field size
				}

				return sizeFromMultipleArgs;
			}

			// Try getting the default value of the size contructor parameter since there were no suitable int arguments passed to the constructor
			var sizeParamFromAttrConstructor = aggregatedAttribute.AttributeConstructor.Parameters[indexOfSizeParamFromAttrConstructor];

			if (sizeParamFromAttrConstructor.HasExplicitDefaultValue && sizeParamFromAttrConstructor.ExplicitDefaultValue is int defaultFieldSize)
				return defaultFieldSize;

			return null; 
		}

		private static bool IsAttributeWithLengthProperty(AttributeData aggregatedAttribute, PXContext pxContext) =>
				pxContext.FieldAttributes.DataAttributesWithLength
										 .Contains<INamedTypeSymbol>(aggregatedAttribute.AttributeClass!, SymbolEqualityComparer.Default);
	}
}
