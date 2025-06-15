using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public readonly struct GlobalSuppressionFileInitInfo : IEquatable<GlobalSuppressionFileInitInfo>
	{
		public string Path { get; }

		public GlobalSuppressionWorkMode SuppressionWorkMode { get; }

		public GlobalSuppressionFileInitInfo(string path, GlobalSuppressionWorkMode suppressionWorkMode)
		{
			Path = path.CheckIfNullOrWhiteSpace();
			SuppressionWorkMode = suppressionWorkMode;
		}

		public override bool Equals(object obj) => 
			obj is GlobalSuppressionFileInitInfo initInfo && Equals(initInfo);

		public bool Equals(GlobalSuppressionFileInitInfo other) => 
			SuppressionWorkMode == other.SuppressionWorkMode && 
			string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (Path?.ToUpperInvariant().GetHashCode() ?? 0);
				hash = 23 * hash + SuppressionWorkMode.GetHashCode();
			}

			return hash;
		}
	}
}
