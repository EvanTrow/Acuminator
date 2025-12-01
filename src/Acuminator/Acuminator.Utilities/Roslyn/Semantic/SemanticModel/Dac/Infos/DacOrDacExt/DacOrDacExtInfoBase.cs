using System;

using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public abstract class DacOrDacExtInfoBase<TInfo> : DacOrDacExtInfoBase, IWriteableBaseItem<TInfo>, IInferredAcumaticaFrameworkTypeInfo
	where TInfo : DacOrDacExtInfoBase<TInfo>
	{
		public new TInfo? Base => base.Base as TInfo;

		TInfo? IWriteableBaseItem<TInfo>.Base
		{
			get => Base;
			set 
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

		/// <inheritdoc path="/summary"/>
		/// <remarks>
		/// <inheritdoc path="/remarks"/>
		/// <br/>
		/// DAC and constructed DAC extension info can't have circular references.
		/// </remarks>
		bool IInferredAcumaticaFrameworkTypeInfo.HasCircularReferences => false;

		/// <inheritdoc path="/summary"/>
		/// <remarks>
		/// <inheritdoc path="/remarks"/>
		/// <br/><br/>
		/// DAC and constructed DAC extension info can't have multiple root types.
		/// </remarks>
		bool IInferredAcumaticaFrameworkTypeInfo.HasMultipleRootTypes => false;

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