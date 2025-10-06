#nullable enable
using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	// Acuminator disable once PX1067 MissingBqlFieldRedeclarationInDerivedDac No need in Acuminator Tests
	[PXCacheName("DAC with All Mandatory Fields")]
	public class DacWithInheritedMandatoryFields : DacWithAllMandatoryFields
	{
		
	}
}