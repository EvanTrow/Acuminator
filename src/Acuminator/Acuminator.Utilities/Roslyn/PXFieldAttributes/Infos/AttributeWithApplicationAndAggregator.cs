using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the attribute with attribute application and an optional aggregator attribute.
	/// </summary>
	public class AttributeWithApplicationAndAggregator : IEquatable<AttributeWithApplicationAndAggregator>
	{
		public ITypeSymbol Type { get; }

		public AttributeData Application { get; }

		[MemberNotNullWhen(true, nameof(Aggregator), nameof(AggregatorType), nameof(AggregatorApplication))]
		public bool HasAggregator => Aggregator != null;

		public AttributeWithApplicationAndAggregator? Aggregator { get; }

		public ITypeSymbol? AggregatorType => Aggregator?.Type;

		public AttributeData? AggregatorApplication => Aggregator?.Application;

		public AttributeWithApplicationAndAggregator(AttributeData attributeApplication, 
													 AttributeWithApplicationAndAggregator? aggregator) : 
												this(attributeApplication, attributeApplication?.AttributeClass!, aggregator)
		{
		}

		public AttributeWithApplicationAndAggregator(AttributeData attributeApplication, ITypeSymbol attributeType, 
													 AttributeWithApplicationAndAggregator? aggregator)
		{
			Application = attributeApplication.CheckIfNull();
			Type 		= attributeType.CheckIfNull();
			Aggregator 	= aggregator;
		}

		public override bool Equals(object obj) => Equals(obj as AttributeWithApplicationAndAggregator);

		public bool Equals(AttributeWithApplicationAndAggregator? other)
		{
			if (other == null || !Type.Equals(other.Type, SymbolEqualityComparer.Default) || !Application.Equals(other.Application))
				return false;

			if (HasAggregator)
			{
				return AggregatorType.Equals(other.AggregatorType, SymbolEqualityComparer.Default) &&
					   AggregatorApplication.Equals(other.AggregatorApplication);
			}
			else
				return !other.HasAggregator;
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(Type);
				hash = 23 * hash + Application.GetHashCode();

				if (HasAggregator)
				{
					hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(AggregatorType);
					hash = 23 * hash + AggregatorApplication.GetHashCode();
				}
			}

			return hash;
		}

		public void Deconstruct(out ITypeSymbol attributeType, out AttributeData attributeApplication)
		{
			attributeType = Type;
			attributeApplication = Application;
		}

		public void Deconstruct(out ITypeSymbol attributeType, out AttributeData attributeApplication, 
								out AttributeWithApplicationAndAggregator? aggregator)
		{
			attributeType 		 = Type;
			attributeApplication = Application;
			aggregator 			 = Aggregator;
		}

		public override string ToString() =>
			HasAggregator
				? $"{Type}: {Application}, Aggregator -> {AggregatorType}: {AggregatorApplication}"
				: $"{Type}: {Application}";
	}
}
