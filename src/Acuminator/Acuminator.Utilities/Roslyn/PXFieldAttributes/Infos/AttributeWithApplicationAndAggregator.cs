#nullable enable

using System;
using System.Collections.Generic;

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

		public AttributeWithApplicationAndAggregator(AttributeData attributeApplication) : this(attributeApplication, attributeApplication?.AttributeClass!)
		{
		}

		public AttributeWithApplicationAndAggregator(AttributeData attributeApplication, ITypeSymbol attributeType)
		{
			Application = attributeApplication.CheckIfNull();
			Type = attributeType.CheckIfNull();
		}

		public override bool Equals(object obj) => obj is AttributeWithApplicationAndAggregator other && Equals(other);

		public bool Equals(AttributeWithApplicationAndAggregator other) =>
			Type.Equals(other.Type, SymbolEqualityComparer.Default) && Application.Equals(other.Application);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + SymbolEqualityComparer.Default.GetHashCode(Type);
				hash = 23 * hash + Application.GetHashCode();
			}

			return hash;
		}

		public void Deconstruct(out ITypeSymbol attributeType, out AttributeData attributeApplication)
		{
			attributeType = Type;
			attributeApplication = Application;
		}

		public override string ToString() => $"{Type}: {Application}";
	}
}
