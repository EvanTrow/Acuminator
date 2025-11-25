using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public readonly struct GlobalSuppressionFileInitInfo : IEquatable<GlobalSuppressionFileInitInfo>
	{
		public string Path { get; }

		/// <inheritdoc cref="SuppressionFile.WorkMode"/>
		public AcuminatorWorkMode WorkMode { get; }

		/// <inheritdoc cref="SuppressionFile.SuppressInformationalDiagnostics"/>
		public bool SuppressInformationalDiagnostics { get; }

		public GlobalSuppressionFileInitInfo(string path, AcuminatorWorkMode workMode, bool suppressInformationalDiagnostics)
		{
			Path = path.CheckIfNullOrWhiteSpace();
			WorkMode = workMode;
			SuppressInformationalDiagnostics = suppressInformationalDiagnostics;
		}

		public override bool Equals(object obj) => 
			obj is GlobalSuppressionFileInitInfo initInfo && Equals(initInfo);

		public bool Equals(GlobalSuppressionFileInitInfo other) => 
			WorkMode == other.WorkMode &&
			SuppressInformationalDiagnostics == other.SuppressInformationalDiagnostics &&
			string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (Path?.ToUpperInvariant().GetHashCode() ?? 0);
				hash = 23 * hash + WorkMode.GetHashCode();
				hash = 23 * hash + SuppressInformationalDiagnostics.GetHashCode();
			}

			return hash;
		}
	}
}
