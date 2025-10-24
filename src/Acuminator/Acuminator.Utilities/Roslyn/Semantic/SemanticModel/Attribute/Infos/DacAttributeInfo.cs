#nullable enable

using System.ComponentModel;
using System.Linq;

using Microsoft.CodeAnalysis;

using static Acuminator.Utilities.Roslyn.Constants.PropertyNames;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about an attribute of a DAC or a DAC extension.
	/// </summary>
	public class DacAttributeInfo : AttributeInfoBase
	{
		public override AttributePlacement Placement => AttributePlacement.Dac;

		/// <summary>
		/// An indicator of whether the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// An indicator of whether the attribute configures a projection DAC.
		/// </summary>
		public bool IsPXProjection { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXCacheName attribute.
		/// </summary>
		public bool IsPXCacheName { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXHidden attribute.
		/// </summary>
		public bool IsPXHidden { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXAccumulatorAttribute attribute.
		/// </summary>
		public bool IsPXAccumulatorAttribute { get; }

		public DacAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			if (AttributeType != null)
			{
				IsDefaultNavigation = AttributeType.IsDefaultNavigation(pxContext);
				(IsPXProjection, IsPXCacheName, IsPXHidden, IsPXAccumulatorAttribute) = GetAttributeFlags(AttributeType, pxContext);
			}
		}

		private static (bool IsProjection, bool IsPXCacheName, bool IsPXHidden, bool IsPXAccumulator) GetAttributeFlags(INamedTypeSymbol attributeType, 
																														PXContext pxContext)
		{
			var projectionAttribute    = pxContext.AttributeTypes.PXProjectionAttribute;
			var pxCacheNameAttribute   = pxContext.AttributeTypes.PXCacheNameAttribute;
			var pxHiddenAttribute 	   = pxContext.AttributeTypes.PXHiddenAttribute;
			var pxAccumulatorAttribute = pxContext.AttributeTypes.PXAccumulatorAttribute;

			bool isPXProjection  = attributeType.Equals(projectionAttribute, SymbolEqualityComparer.Default);
			bool isPXCacheName 	 = attributeType.Equals(pxCacheNameAttribute, SymbolEqualityComparer.Default);
			bool isPXHidden 	 = attributeType.Equals(pxHiddenAttribute, SymbolEqualityComparer.Default);
			bool isPXAccumulator = attributeType.Equals(pxAccumulatorAttribute, SymbolEqualityComparer.Default);

			if (isPXProjection || isPXCacheName || isPXHidden || isPXAccumulator)
				return (isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);

			var attributeBaseTypes = attributeType.GetBaseTypes();

			foreach (var baseType in attributeBaseTypes)
			{
				if (baseType.SpecialType == SpecialType.System_Object)
					break;

				isPXProjection 	= isPXProjection  || attributeType.Equals(projectionAttribute, SymbolEqualityComparer.Default);
				isPXCacheName  	= isPXCacheName   || attributeType.Equals(pxCacheNameAttribute, SymbolEqualityComparer.Default);
				isPXHidden	   	= isPXHidden 	  || attributeType.Equals(pxHiddenAttribute, SymbolEqualityComparer.Default);
				isPXAccumulator = isPXAccumulator || attributeType.Equals(pxAccumulatorAttribute, SymbolEqualityComparer.Default);

				if (isPXProjection || isPXCacheName || isPXHidden || isPXAccumulator)
					return (isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);
			}

			return (isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);
		}
	}
}