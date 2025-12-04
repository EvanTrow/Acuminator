using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public sealed class DacExtensionInfo : DacOrDacExtInfoBase<DacExtensionInfo>, IExtensionInfo<DacExtensionInfo>
	{
		public DacInfo? Dac { get; }

		public ExtensionMechanismType BaseExtensionsMechanismType { get; }

		ImmutableArray<DacExtensionInfo> IExtensionInfo<DacExtensionInfo>.BaseExtensions =>
			Base != null 
				? [Base]
				: ImmutableArray<DacExtensionInfo>.Empty;

		internal DacExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol dacExtension, DacInfo? dac, int declarationOrder, 
								  DacExtensionInfo baseInfo, ExtensionMechanismType extensionMechanismType) :
							 base(node, dacExtension, declarationOrder, baseInfo)
		{
			Dac = dac;
			BaseExtensionsMechanismType = extensionMechanismType;

			CombineWithBaseInfo();
		}

		internal DacExtensionInfo(ClassDeclarationSyntax? node, ITypeSymbol dacExtension, DacInfo? dac, int declarationOrder) :
							 base(node, dacExtension, declarationOrder)
		{
			Dac = dac;
			BaseExtensionsMechanismType = ExtensionMechanismType.None;

			CombineWithBaseInfo();
		}

		public override IEnumerable<DacOrDacExtInfoBase> GetInfosFromDerivedExtensionToBaseDac(bool includeSelf)
		{
			var dacExtensionInfos = includeSelf
				? this.ThisAndOverriddenItems()
				: this.JustOverriddenItems();

			if (Dac != null)
			{
				var dacInfos = Dac.GetInfosFromDerivedExtensionToBaseDac(includeSelf: true);
				return dacExtensionInfos.Concat(dacInfos);
			}
			else
				return dacExtensionInfos;
		}

		protected override void CombineWithBaseInfo()
		{
			if (Base != null)
				CombineWithBaseDacExtension();
			else
				CombineWithBaseDac();
		}

		private void CombineWithBaseDacExtension() { }

		private void CombineWithBaseDac() { }
	}
}