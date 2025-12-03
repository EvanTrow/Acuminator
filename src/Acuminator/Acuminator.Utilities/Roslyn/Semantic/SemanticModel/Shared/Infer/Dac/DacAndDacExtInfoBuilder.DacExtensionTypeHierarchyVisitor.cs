using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Dac;

public partial class DacAndDacExtInfoBuilder : SymbolInfoBuilderBase<DacInfo, DacExtensionInfo>
{
	private class DacExtensionTypeHierarchyVisitor : ExtensionTypeHierarchyVisitor
	{
		public DacExtensionTypeHierarchyVisitor(DacAndDacExtInfoBuilder builder, PXContext pxContext, CancellationToken cancellation) : 
											base(builder, pxContext, cancellation)
		{
		}
	}
}