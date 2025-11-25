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
		/// Property stores the <see cref="BaseDelegateParameterFixMode"/> value for a patch method.
		/// </summary>
		public const string DelegateParameterFixMode = nameof(DelegateParameterFixMode);

		/// <summary>
		/// Property stores the XML documentation comment ID name of the base method.
		/// </summary>
		public const string BaseMethodDocCommentId = nameof(BaseMethodDocCommentId);
	}
}