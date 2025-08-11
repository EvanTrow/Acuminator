namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	/// <summary>
	/// Values that represent possible ways to specify a virtual type member.
	/// </summary>
	public enum MemberVirtualityKind : byte
	{
		/// <summary>
		/// The method is not virtual.
		/// </summary>
		None,
		
		/// <summary>
		/// The virtual method is specified with a <see langword="virtual"/> modifier.
		/// </summary>
		Virtual,

		/// <summary>
		///  The virtual method is specified with a <see langword="abstract"/> modifier.
		/// </summary>
		Abstract,

		/// <summary>
		/// The virtual method is specified with a <see langword="override"/> modifier.
		/// </summary>
		Override,

		/// <summary>
		/// The virtual method is specified with <see langword="sealed"/> and <see langword="override"/> modifiers.
		/// </summary>
		SealedOverride,
	}
}