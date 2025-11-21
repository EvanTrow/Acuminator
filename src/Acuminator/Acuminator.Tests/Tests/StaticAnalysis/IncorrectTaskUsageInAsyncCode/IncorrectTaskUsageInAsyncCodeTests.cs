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

		[Theory]
		[EmbeddedFileData(@"StoringTaskInVariable.cs")]
		public Task Storing_TaskInstance_InLocalVariable(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(9, 20),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(9, 39),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(10, 20),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(15, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(15, 16),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(19, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(19, 31),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(23, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(23, 26),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(23, 64),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(27, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(27, 44),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(30, 4));

		[Theory]
		[EmbeddedFileData(@"TaskInParameters.cs")]
		public Task Parameter_With_Task_Type(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(7, 25),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(9, 59),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(13, 19),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(15, 20),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(19, 39),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(24, 39),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(24, 59),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable.CreateFor(24, 76));

		[Theory]
		[EmbeddedFileData(@"NotAwaitedTaskReturningExpression.cs", @"NotAwaitedTaskReturningExpression.ExternalHelper.cs")]
		public Task NotAwaitedTaskReturningExpression(string source, string additionalSource) =>
			VerifyCSharpDiagnosticAsync(source, additionalSource,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(12, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(15, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(18, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(21, 4),

				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(30, 36),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(31, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(32, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(32, 30),

				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(35, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(36, 10),

				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(38, 4),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(38, 44),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(39, 50),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression.CreateFor(40, 32));

		[Theory]
		[EmbeddedFileData(@"MethodReturnTypeIsNotTask.cs")]
		public Task MethodReturnTypeIsNotTask(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(12, 11),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(18, 11),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(22, 33),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(25, 43),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(28, 38),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(39, 12),
				Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask.CreateFor(42, 31));

		[Theory]
		[EmbeddedFileData(@"CorrectTaskUsage.cs")]
		public Task CorrectTaskUsage_NoDiagnostic(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"CorrectTaskUsageInLongOperationAPIs.cs")]
		public Task CorrectTaskUsage_InLongOperationAPIs_NoDiagnostic(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"NonAsyncCode.cs")]
		public Task NonAsyncCode_NoDiagnostic(string source) => VerifyCSharpDiagnosticAsync(source);
	}
}
