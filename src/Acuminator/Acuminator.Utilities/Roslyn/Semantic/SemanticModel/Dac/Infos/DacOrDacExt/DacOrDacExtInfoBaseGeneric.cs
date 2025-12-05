using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public abstract class DacOrDacExtInfoBase<TInfo> : DacOrDacExtInfoBase, IWriteableBaseItem<TInfo>, IInferredAcumaticaSymbolInfo
	where TInfo : DacOrDacExtInfoBase<TInfo>
	{
		public new TInfo? Base => base.Base as TInfo;

		TInfo? IWriteableBaseItem<TInfo>.Base
		{
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

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder, TInfo baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, ITypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}

		void IOverridableItem<TInfo>.CombineWithBaseInfo() => CombineWithBaseInfo();
	}
}