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
		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, DacOrDacExtInfoBase baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
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

			{
				if (this is IWriteableBaseItem<DacOrDacExtInfoBase> baseInterface)
					baseInterface.Base = value;
				else
				{
					_baseInfo = value;

					if (value != null)
						CombineWithBaseInfo();
				}
			}
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, TInfo baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}

		void IOverridableItem<TInfo>.CombineWithBaseInfo() => CombineWithBaseInfo();

		protected sealed override void CombineWithBaseInfo()
		{
			if (_baseInfo == null)
				return;
			else if (_baseInfo is not TInfo)
			{
				throw new ArgumentOutOfRangeException(nameof(_baseInfo),
								$"Type \"{_baseInfo.GetType().FullName}\" is not \"{typeof(TInfo).FullName}\" or derived from it.");
			}
		}
	}


	public abstract class DacOrDacExtInfoBase : OverridableNodeSymbolItem<DacOrDacExtInfoBase, ClassDeclarationSyntax, ITypeSymbol>
	{
		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, DacOrDacExtInfoBase baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}
	}
}