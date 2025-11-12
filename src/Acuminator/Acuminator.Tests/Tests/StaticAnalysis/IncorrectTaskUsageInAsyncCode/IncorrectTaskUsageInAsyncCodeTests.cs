using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.IncorrectTaskUsageInAsyncCode;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using AnalyzerResources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.IncorrectTaskUsageInAsyncCode
{
	public class IncorrectTaskUsageInAsyncCodeTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new IncorrectTaskUsageInAsyncCodeAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
																				  .WithSuppressionMechanismDisabled());
		
		
	}
}
