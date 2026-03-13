using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Runner.Constants
{
	internal static class Constant
	{
		public static class Common
		{
			public const string ProjectFileExtension = ".csproj";
			public const string SolutionFileExtension = ".sln";
		}

		public static class Output
		{
			public const string LinePartsSeparator = ": ";
			public const string SeverityTemplate   = "[{0}] ";
			public const string SeverityError 	   = "ERROR";
			public const string SeverityWarning    = "WARNING";
			public const string SeverityInfo 	   = "INFO";
		}
	}
}
