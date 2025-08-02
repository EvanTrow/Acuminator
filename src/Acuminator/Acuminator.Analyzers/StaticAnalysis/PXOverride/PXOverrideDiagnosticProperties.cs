namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	internal static class PXOverrideDiagnosticProperties
	{
		/// <summary>
		/// Indicates whether the patch method is a non-public method.
		/// </summary>
		public const string IsNonPublicPatchMethod = nameof(IsNonPublicPatchMethod);

		/// <summary>
		/// Property stores the <see cref="MemberVirtualityKind"/> value for a patch method.
		/// </summary>
		public const string PatchMethodVirtualityKind = nameof(PatchMethodVirtualityKind);
	}
}