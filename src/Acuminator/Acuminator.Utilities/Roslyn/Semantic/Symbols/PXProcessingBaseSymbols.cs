using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXProcessingBaseSymbols : SymbolsSetForTypeBase
	{
		public IMethodSymbol SetParametersDelegate { get; }

		public ImmutableArray<IMethodSymbol> SetProcessDelegate { get; }

		public ImmutableArray<IMethodSymbol> SetAsyncProcessDelegate { get; }

		internal PXProcessingBaseSymbols(Compilation compilation) : base(compilation, typeName: TypeFullNames.PXProcessingBase)
		{
			SetParametersDelegate 	= Type.GetMethods(DelegateNames.Processing.SetParameters).First();
			SetProcessDelegate 		= Type.GetMethods(DelegateNames.Processing.SetProcessDelegate).ToImmutableArray();
			SetAsyncProcessDelegate = Type.GetMethods(DelegateNames.Processing.SetAsyncProcessDelegate).ToImmutableArray();
		}
	}
}
