using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output.Data
{
	internal class CodeSourceReport
	{
		public string CodeSourceName { get; }

		public int TotalErrorCount { get; }

		public IReadOnlyCollection<ProjectReport> ProjectReports { get; }

		public CodeSourceReport(string codeSourceName, IEnumerable<ProjectReport> projectReports)
		{
			CodeSourceName 	= codeSourceName.CheckIfNullOrWhiteSpace();
			ProjectReports 	= projectReports.CheckIfNullOrEmpty().ToList();
			TotalErrorCount = ProjectReports.Sum(report => report.TotalErrorsCount);
		}
	}
}
