using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output
{
	internal class ProjectReport
	{
		public string ProjectName { get; }

		public required int TotalErrorCount { get; init; }

		public required int DistinctApisCount { get; init; }

		public required ReportGroup? ReportDetails { get; init; }

        public ProjectReport(string projectName)
        {
            ProjectName = projectName.CheckIfNullOrWhiteSpace();
        }

		public static ProjectReport SuccessfulReport(string projectName)
		{
			var title = string.Format(CultureInfo.CurrentCulture, Messages.AcuminatorValidationPassedMessage, projectName);
			return new ProjectReport(projectName)
			{
				TotalErrorCount   = 0,
				DistinctApisCount = 0,
				ReportDetails 	  = new ReportGroup
				{
					DistinctApisCount = 0,
					TotalErrorCount   = 0,
					GroupTitle 		  = new Title(title, TitleKind.File)
				}
			};
		}

		[MemberNotNullWhen(returnValue: false, nameof(ReportDetails))]
		public bool IsEmptyReport() => ReportDetails == null ||
			(ReportDetails.GroupTitle == null &&
			 ReportDetails.Lines?.Count is null or 0 && ReportDetails.LinesTitle == null &&
			 ReportDetails.ChildrenGroups?.Count is null or 0 && ReportDetails.ChildrenTitle == null);
    }
}
