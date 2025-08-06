namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	internal static class PXOverrideDiagnosticProperties
	{
		/// <summary>
		/// Property indicates whether the patch method is a non-public method.
		/// </summary>
		public const string IsNonPublicPatchMethod = nameof(IsNonPublicPatchMethod);

		/// <summary>
		/// Property stores the <see cref="MemberVirtualityKind"/> value for a patch method.
		/// </summary>
		public const string PatchMethodVirtualityKind = nameof(PatchMethodVirtualityKind);

		/// <summary>
		/// Property stores the name of the patch method.
		/// </summary>
		public const string PatchMethodName = nameof(PatchMethodName);

		/// <summary>
		/// Property indicates whether the code fix shared by PX1079 and PX1101 diagnostics should replace the last delegate parameter.
		/// </summary>
		public const string ReplaceLastDelegateParameter = nameof(ReplaceLastDelegateParameter);
	}
}