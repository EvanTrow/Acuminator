namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	/// <summary>
	/// Values that represent different modes for code fix of the base delegate parameter.
	/// </summary>
	public enum BaseDelegateParameterFixMode : byte
	{
		/// <summary>
		/// Code fix shoud add a base delegate parameter to the method.
		/// </summary>
		AddDelegateParameter,

		/// <summary>
		/// Code fix shoud replace a base delegate parameter of the method with a generated correct delegate parameter.
		/// </summary>
		ReplaceDelegateParameter,

		/// <summary>
		/// Code fix shoud rename a base delegate parameter of the method with a correct name.
		/// </summary>
		RenameDelegateParameter
	}
}