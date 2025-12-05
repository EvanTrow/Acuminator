using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public abstract class DacOrDacExtInfoBase : OverridableNodeSymbolItem<DacOrDacExtInfoBase, ClassDeclarationSyntax, ITypeSymbol>
	{
		public abstract ITypeSymbol? DacType { get; }

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dacOrDacExt, int declarationOrder, DacOrDacExtInfoBase baseInfo) :
								 base(node, dacOrDacExt, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dacOrDacExt, int declarationOrder) :
								 base(node, dacOrDacExt, declarationOrder)
		{
		}

		/// <summary>
		/// Gets the infos from derived DAC extension to base DAC.
		/// </summary>
		/// <param name="includeSelf">True to include base infos, false to exclude them.</param>
		/// <returns/>
		public abstract IEnumerable<DacOrDacExtInfoBase> GetInfosFromDerivedExtensionToBaseDac(bool includeSelf);

		/// <summary>
		/// Gets the infos from base DAC to derived DAC extension.
		/// </summary>
		/// <param name="includeSelf">True to include base infos, false to exclude them.</param>
		/// <returns/>
		public IEnumerable<DacOrDacExtInfoBase> GetInfosFromBaseDacToDerivedExtension(bool includeSelf) =>
			GetInfosFromDerivedExtensionToBaseDac(includeSelf).Reverse();

		internal OverridableItemsCollection<DacBqlFieldInfo> GetDacBqlFieldInfos(PXContext pxContext, CancellationToken cancellation)
		{
			int estimatedCapacity = DacType?.GetTypeMembers().Length ?? 0;
			var bqlFieldsByName = new OverridableItemsCollection<DacBqlFieldInfo>(estimatedCapacity);
			var rawBqlFieldsDataFromBaseDacToDerivedExtension = GetRawBqlFieldsData(pxContext, includeFromBaseInfos: true, cancellation);

			int declarationOrder = 0;

			foreach (var (bqlFieldNode, bqlFieldSymbol) in rawBqlFieldsDataFromBaseDacToDerivedExtension)
			{
				cancellation.ThrowIfCancellationRequested();
				var propertyInfo = DacBqlFieldInfo.CreateUnsafe(pxContext, bqlFieldNode, bqlFieldSymbol, declarationOrder);

				bqlFieldsByName.Add(propertyInfo);
				declarationOrder++;
			}

			return bqlFieldsByName;
		}

		internal OverridableItemsCollection<DacPropertyInfo> GetPropertyInfos(PXContext pxContext, IDictionary<string, DacBqlFieldInfo> dacFields,
																			  CancellationToken cancellation)
		{
			var dbBoundnessCalculator = new DbBoundnessCalculator(pxContext);
			int estimatedCapacity = DacType?.GetTypeMembers().Length ?? 0;
			var propertiesByName = new OverridableItemsCollection<DacPropertyInfo>(estimatedCapacity);
			var rawPropertiesDataFromBaseDacToDerivedExtension = GetRawPropertiesData(pxContext, includeFromBaseInfos: true, cancellation);

			int declarationOrder = 0;

			foreach (var (propertyNode, propertySymbol) in rawPropertiesDataFromBaseDacToDerivedExtension)
			{
				cancellation.ThrowIfCancellationRequested();
				var propertyInfo = DacPropertyInfo.CreateUnsafe(pxContext, propertyNode, propertySymbol, declarationOrder,
																dbBoundnessCalculator, dacFields);
				propertiesByName.Add(propertyInfo);
				declarationOrder++;
			}

			return propertiesByName;
		}

		/// <summary>
		/// Get all properties with nodes from DAC or DAC extension and its base infos.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="includeFromBaseInfos">True to include, false to exclude DAC properties from base infos.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns/>
		private IEnumerable<(PropertyDeclarationSyntax? Node, IPropertySymbol Symbol)> GetRawPropertiesData(PXContext pxContext,
																											bool includeFromBaseInfos,
																											CancellationToken cancellation) =>
			GetRawData(includeFromBaseInfos,
						dacOrDacExtInfo => GetRawPropertiesData(dacOrDacExtInfo, pxContext, cancellation));
		
		/// <summary>
		/// Get all DAC BQL fields with nodes from DAC or DAC extension and its base infos.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="includeFromBaseInfos">True to include, false to exclude DAC BQL fields from base infos.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns/>
		private IEnumerable<(ClassDeclarationSyntax? Node, INamedTypeSymbol Symbol)> GetRawBqlFieldsData(PXContext pxContext,
																										 bool includeFromBaseInfos,
																										 CancellationToken cancellation) =>
			GetRawData(includeFromBaseInfos, 
						dacOrDacExtInfo => GetRawBqlFieldsData(dacOrDacExtInfo, pxContext, cancellation));
		
		private IEnumerable<TRawData> GetRawData<TRawData>(bool includeFromBaseInfos,
														   Func<DacOrDacExtInfoBase, IEnumerable<TRawData>> rawDataGetter)
		{
			if (includeFromBaseInfos)
			{
				return GetInfosFromBaseDacToDerivedExtension(includeFromBaseInfos)
						  .SelectMany(dacOrDacExtInfo => rawDataGetter(dacOrDacExtInfo));
			}
			else
			{
				return rawDataGetter(this);
			}
		}

		private static IEnumerable<(PropertyDeclarationSyntax? Node, IPropertySymbol Symbol)> GetRawPropertiesData(
																											DacOrDacExtInfoBase dacOrDacExtInfo,
																											PXContext pxContext, 
																											CancellationToken cancellation)
		{
			var dacProperties = dacOrDacExtInfo.Symbol.GetMembers()
													  .OfType<IPropertySymbol>()
													  .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

			foreach (IPropertySymbol property in dacProperties)
			{
				cancellation.ThrowIfCancellationRequested();

				var propertyNode = property.GetSyntax(cancellation) as PropertyDeclarationSyntax;
				yield return (propertyNode, property);
			}
		}

		private static IEnumerable<(ClassDeclarationSyntax? Node, INamedTypeSymbol Symbol)> GetRawBqlFieldsData(
																									DacOrDacExtInfoBase dacOrDacExtInfo,
																									PXContext pxContext, 
																									CancellationToken cancellation)
		{
			var dacBqlFields = dacOrDacExtInfo.Symbol.GetTypeMembers()
													 .Where(type => type.IsDacBqlField(pxContext));

			foreach (INamedTypeSymbol bqlField in dacBqlFields)
			{
				cancellation.ThrowIfCancellationRequested();
				var bqlFieldNode = bqlField.GetSyntax(cancellation) as ClassDeclarationSyntax;

				yield return (bqlFieldNode, bqlField);
			}
		}
	}
}