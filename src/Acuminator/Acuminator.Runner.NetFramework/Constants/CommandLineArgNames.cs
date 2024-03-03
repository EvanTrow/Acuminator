using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Runner.Constants
{
	internal static class CommandLineArgNames
	{
		public const string CodeSource = "codeSource";

		//public const char VerbosityShort = 'v';
		//public const string VerbosityLong = "verbosity";

		public const string DisableSuppressionMechanism = "noSuppression";
		public const string MSBuildPath = "msBuildPath";

		public const string OutputAbsolutePathsToUsages = "outputAbsolutePaths";

		public const char OutputFileShort = 'f';
		public const string OutputFileLong = "file";

		public const string OutputFormat = "format";
	}
}