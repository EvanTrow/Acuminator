using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacExtensionInfo : DacOrDacExtInfoBase<DacExtensionInfo>
	{
		public DacInfo? Dac { get; }

		public ExtensionMechanismType BaseExtensionsMechanismType { get; }

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

		public static DacExtensionInfo? Create(ITypeSymbol? dacExtension, ClassDeclarationSyntax? dacExtensionNode, ITypeSymbol? dac, 
											   PXContext pxContext, int dacExtDeclarationOrder, CancellationToken cancellation)
		{
			if (dacExtension == null)
				return null;

			cancellation.ThrowIfCancellationRequested();

			var dacNode = dac.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var dacInfo = DacInfo.Create(dac, dacNode, pxContext, dacDeclarationOrder: 0, cancellation);
			
			var extensionTypesFromFirstToLastLevel = dacExtension.GetBaseExtensions(pxContext, SortDirection.Ascending, includeDac: false);
			DacExtensionInfo? aggregatedBaseDacInfo = null, prevDacInfo = null;

			foreach (ITypeSymbol baseExtensionType in extensionTypesFromFirstToLastLevel)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseDacExtNode = baseExtensionType.GetSyntax(cancellation) as ClassDeclarationSyntax;

				aggregatedBaseDacInfo = prevDacInfo != null
					? new DacExtensionInfo(baseDacExtNode, baseExtensionType, dacInfo, declarationOrder: 1, prevDacInfo)
					: new DacExtensionInfo(baseDacExtNode, baseExtensionType, dacInfo, declarationOrder: 1);

				prevDacInfo = aggregatedBaseDacInfo;
			}

			var dacExtensionInfo = aggregatedBaseDacInfo != null
				? new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, dacExtDeclarationOrder, aggregatedBaseDacInfo)
				: new DacExtensionInfo(dacExtensionNode, dacExtension, dacInfo, dacExtDeclarationOrder);

			return dacExtensionInfo;
		}
	}
}