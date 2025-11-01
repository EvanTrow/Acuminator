using PX.Data;

namespace Acuminator.Tests.Sources
{
	/// <exclude/>
	[PXCacheName("Empty DAC - should not be checked")]
	public class EmptyDac : IBqlTable
	{
		// No fields defined - should not trigger PX1069 because analyzer checks DacFieldsByNames.Count > 0
	}
}