using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class ActionHandlerReturnTypeFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1013_PXActionHandlerInvalidReturnType.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var changeReturnTypeTitle = nameof(Resources.PX1013Fix).GetLocalized().ToString();
			var codeFixAction = new InvalidPXActionSignatureFix.ChangeSignatureAction(changeReturnTypeTitle, context.Document, context.Span);

			context.RegisterCodeFix(codeFixAction, diagnostic);
			return Task.CompletedTask;
		}
	}
}
