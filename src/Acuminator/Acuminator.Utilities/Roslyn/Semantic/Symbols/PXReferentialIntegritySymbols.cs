using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// A referential integrity symbols related to Acumatica PK/FK API.
	/// </summary>
	public class PXReferentialIntegritySymbols : SymbolsSetBase
	{
		public static ImmutableHashSet<string> ForeignKeyContainerNames { get; } =
			new HashSet<string>
			{
				TypeNames.ReferentialIntegrity.AsSimpleKeyName,
				TypeNames.ReferentialIntegrity.ForeignKeyOfName,
				TypeNames.ReferentialIntegrity.CompositeKey
			}
			.ToImmutableHashSet();

		/// <summary>
		/// The maximum size of the DAC primary key.
		/// </summary>
		/// <remarks>
		/// The size of a primary key was increased in Acumatica 2020r201. Earlier versions of Acumatica have max primary key size 8.
		/// </remarks>
		public const int MaxPrimaryKeySize = 12;

		/// <summary>
		/// Gets the primary key interface.
		/// </summary>
		/// <value>
		/// The primary key interface.
		/// </value>
		public INamedTypeSymbol? IPrimaryKey { get; }

		/// <summary>
		/// Gets the generic IPrimaryKeyOf<TDAC> interface.
		/// </summary>
		/// <value>
		/// The generic IPrimaryKeyOf<TDAC> interface.
		/// </value>
		public INamedTypeSymbol? IPrimaryKeyOf1 { get; }

		/// <summary>
		/// Gets the foreign key interface. For earlier versions of Acumatica (2019R1) is not defined so it can be null.
		/// </summary>
		/// <value>
		/// The foreign key interface.
		/// </value>
		public INamedTypeSymbol? IForeignKey { get; }

		/// <summary>
		/// Gets the generic foreign key to the parent DAC interface derived from <see cref="IForeignKey"/>. Contains information about parent DAC referenced by the foreign key.
		/// For earlier versions of Acumatica (2019R1) the interface is not defined so the symbol can be null.
		/// </summary>
		/// <value>
		/// The generic foreign key to the parent DAC interface
		/// </value>
		public INamedTypeSymbol? IForeignKeyTo1 { get; }

		public INamedTypeSymbol? KeysRelation { get; }

		public INamedTypeSymbol? PrimaryKeyOf => Compilation.GetTypeByMetadataName(TypeFullNames.PrimaryKeyOf);

		public INamedTypeSymbol? CompositeKey2 => Compilation.GetTypeByMetadataName(TypeFullNames.CompositeKey2);

		internal PXReferentialIntegritySymbols(Compilation compilation) : base(compilation)
		{
			IPrimaryKey = Compilation.GetTypeByMetadataName(TypeFullNames.IPrimaryKey);
			IPrimaryKeyOf1 = Compilation.GetTypeByMetadataName(TypeFullNames.IPrimaryKeyOf1);

			IForeignKey = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKey);
			IForeignKeyTo1 = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKeyTo1);

			KeysRelation = Compilation.GetTypeByMetadataName(TypeFullNames.KeysRelation);
		}

		public INamedTypeSymbol? GetPrimaryKeyBy_TypeSymbol(int arity)
		{
			switch (arity)
			{
				case < 0:
					throw new ArgumentOutOfRangeException(nameof(arity));
				case 0:
				case > MaxPrimaryKeySize:            // The max size of a primary key may change again in the future, so no exception should be thrown here.
					return null;
			}

			string primaryKeyByTypeName = $"{TypeFullNames.PrimaryKeyOfBy}`{arity}";
			return Compilation.GetTypeByMetadataName(primaryKeyByTypeName);
		}
	}
}
