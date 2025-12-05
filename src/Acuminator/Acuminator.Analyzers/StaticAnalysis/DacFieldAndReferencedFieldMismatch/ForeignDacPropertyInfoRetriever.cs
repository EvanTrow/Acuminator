using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.DacFieldAndReferencedFieldMismatch
{
	internal class ForeignDacPropertyInfoRetriever
	{
		private readonly PXContext _pxContext;
		private readonly CancellationToken _cancellation;

		private readonly Dictionary<INamedTypeSymbol, DacSemanticModel?> _dacModelsCache = new(SymbolEqualityComparer.Default);

		public ForeignDacPropertyInfoRetriever(PXContext pxContext, CancellationToken cancellation)
		{
			_pxContext	  = pxContext;
			_cancellation = cancellation;
		}

		public DacPropertyInfo? GetForeignDacPropertyInfo(DacFieldAttributeInfo foreignReferenceAttribute)
		{
			var foreignDacField = ExtractForeignFieldSymbol(foreignReferenceAttribute);

			if (foreignDacField is null)
				return null;

			var foreignDacType  = foreignDacField.ContainingType!;
			var foreignDacModel = GetOrInferModel(foreignDacType);

			if (foreignDacModel?.PropertiesByNames.TryGetValue(foreignDacField.Name, out var foreignDacProperty) != true)
				return null;

			return foreignDacProperty;
		}

		private INamedTypeSymbol? ExtractForeignFieldSymbol(DacFieldAttributeInfo foreignReferenceAttribute)
		{
			if (foreignReferenceAttribute.AttributeData.ConstructorArguments.IsDefaultOrEmpty)
				return null;

			TypedConstant argument = foreignReferenceAttribute.AttributeData.ConstructorArguments[0];

			// Need to check TypedConstant.Kind first because TypedConstant.Value may throw exception for Array kind
			if (argument.Kind is not TypedConstantKind.Type)
				return null;

			INamedTypeSymbol? foreignFieldType = argument.Value switch
			{
				INamedTypeSymbol { IsGenericType: true } bqlSearchType => ExtractForeignFieldSymbolFromBqlSearch(bqlSearchType),
				INamedTypeSymbol nonGenericFirstConstructorTypeArg	   => nonGenericFirstConstructorTypeArg,
				_													   => null
			};

			return foreignFieldType?.ContainingType != null
				? foreignFieldType
				: null;
		}

		private INamedTypeSymbol? ExtractForeignFieldSymbolFromBqlSearch(INamedTypeSymbol bqlSearchType)
		{
			var typeHierarchy = bqlSearchType.GetBaseTypesAndThis()
											 .TakeWhile(x => x.SpecialType != SpecialType.System_Object)
											 .OfType<INamedTypeSymbol>()
											 .Reverse();

			foreach (var bqlBaseType in typeHierarchy)
			{
				if (bqlBaseType.TypeArguments.IsDefaultOrEmpty || !bqlBaseType.ImplementsInterface(_pxContext.BQL.IBqlSearch))
					continue;

				var bqlField = bqlBaseType.TypeArguments.FirstOrDefault(typeArg => typeArg.IsDacBqlField(_pxContext));

				if (bqlField is INamedTypeSymbol foundBqlField)
					return foundBqlField;
			}

			return null;
		}

		private DacSemanticModel? GetOrInferModel(INamedTypeSymbol dacType)
		{
			if (_dacModelsCache.TryGetValue(dacType, out DacSemanticModel? existingDacModel))
				return existingDacModel;
			else
			{
				var newDacModel = DacSemanticModel.InferModel(_pxContext, dacType, cancellation: _cancellation);
				_dacModelsCache.Add(dacType, newDacModel);
				return newDacModel;
			}
		}
	}
}
