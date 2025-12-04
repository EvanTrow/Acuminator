using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacExtensionInfo : DacOrDacExtInfoBase<DacExtensionInfo>, IExtensionInfo<DacExtensionInfo>
	{
		public DacInfo? Dac { get; }

		public ExtensionMechanismType BaseExtensionsMechanismType { get; }

		ImmutableArray<DacExtensionInfo> IExtensionInfo<DacExtensionInfo>.BaseExtensions =>
			Base != null 
				? [Base]
				: ImmutableArray<DacExtensionInfo>.Empty;

		internal DacExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol dacExtension, DacInfo? dac, int declarationOrder, 
								  DacExtensionInfo baseInfo, ExtensionMechanismType extensionMechanismType) :
						   this(node, dacExtension, dac, declarationOrder)
		{
			BaseExtensionsMechanismType = extensionMechanismType;
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo();
		}

		internal DacExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol dacExtension, DacInfo? dac, int declarationOrder) :
							 base(node, dacExtension, declarationOrder)
		{
			BaseExtensionsMechanismType = ExtensionMechanismType.None;
			Dac = dac;
		}
	}
}