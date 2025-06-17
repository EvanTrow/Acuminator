using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	/// <summary>
	/// Work modes og the global suppression mechanism.
	/// </summary>
	[Flags]
	public enum AcuminatorWorkMode : byte
	{
		/// <summary>
		/// Acuminator will report errors that are not suppressed in the project suppression file.
		/// </summary>
		ReportUnsuppressedErrors = 0b0001,

		/// <summary>
		/// Acuminator will add all found errors to project's suppression file. The errors will not be reported.
		/// </summary>
		GenerateSuppressionFile = 0b0010,

		/// <summary>
		/// Acuminator will both report found errors and add them to the suppression file.
		/// </summary>
		BothReportAndGenerate = ReportUnsuppressedErrors | GenerateSuppressionFile
	}

	/// <summary>
	/// The helper class for <see cref="AcuminatorWorkMode"/>.
	/// </summary>
	public static class AcuminatorWorkModeExtensions
	{
		/// <summary>
		/// Checks if the mode contains the specified flag.
		/// </summary>
		public static bool HasFlag(this AcuminatorWorkMode mode, AcuminatorWorkMode flag) =>
			(mode & flag) == flag;
	}
}
