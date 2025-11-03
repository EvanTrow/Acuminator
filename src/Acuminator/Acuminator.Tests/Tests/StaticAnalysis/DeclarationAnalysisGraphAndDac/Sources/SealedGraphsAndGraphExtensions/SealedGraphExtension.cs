using PX.Data;

namespace PX.Objects
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class SealedGraphExtension : PXGraphExtension<MyGraph>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed partial class SealedPartialGraphExtension : PXGraphExtension<MyGraph> { }

	public sealed partial class SealedPartialGraphExtension : PXGraphExtension<MyGraph> { }

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}