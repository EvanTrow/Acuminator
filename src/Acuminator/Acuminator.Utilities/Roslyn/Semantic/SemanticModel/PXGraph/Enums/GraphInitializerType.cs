namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Values that represent graph initializer types.
	/// </summary>
	public enum GraphInitializerType
	{
		/// <summary>
		/// An enum constant representing the instance constructor.
		/// </summary>
		InstanceConstructor,

		/// <summary>
		/// An enum constant representing the Initialize method.
		/// </summary>
		InitializeMethod,

		/// <summary>
		/// An enum constant representing the instance created delegate.
		/// </summary>
		InstanceCreatedDelegate,

		/// <summary>
		/// An enum constant representing the Configure method.
		/// </summary>
		ConfigureMethod
	}
}
