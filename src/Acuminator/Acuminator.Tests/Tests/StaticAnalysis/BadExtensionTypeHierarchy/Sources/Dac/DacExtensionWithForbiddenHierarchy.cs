using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.BadExtensionTypeHierarchy.Dac.Sources
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class SecondLevelDacExtension : PXCacheExtension<SecondIndependentDacExtension, FirstIndependentDacExtension, MyDac>
	{
	}

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class SecondIndependentDacExtension : PXCacheExtension<MyDac>
	{
	}

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class FirstIndependentDacExtension : PXCacheExtension<MyDac>
	{
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}
