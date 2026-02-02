using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.UseOfNamespaceReservedByAcumatica;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseOfNamespaceReservedByAcumaticaAnalyzer : PXDiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create
		(
			Descriptors.PX1039_UseOfNamespaceReservedByAcumatica
		);

	public UseOfNamespaceReservedByAcumaticaAnalyzer() : base() { }

	/// <summary>
	/// Constructor accepting code analysis settings for tests.
	/// </summary>
	/// <param name="codeAnalysisSettings">The code analysis settings.</param>
	public UseOfNamespaceReservedByAcumaticaAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
	{
	}

	protected override bool ShouldAnalyze(PXContext pxContext) => 
		base.ShouldAnalyze(pxContext) && pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled;

	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
	{
		var fileScopedNamespaceDeclaration = (SyntaxKind)SharedConstants.FileScopedNamespaceDeclarationKind;
		compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeNamespaceDeclaration(syntaxContext, pxContext),
														 SyntaxKind.NamespaceDeclaration, fileScopedNamespaceDeclaration);
	}

	private void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
	{
		syntaxContext.CancellationToken.ThrowIfCancellationRequested();
		
		if (IsAcumaticaNamespaceUsed(pxContext, syntaxContext.Node))
		{
			var identifier = syntaxContext.Node.ChildNodes().OfType<QualifiedNameSyntax>().FirstOrDefault<SyntaxNode>() ??
							 syntaxContext.Node.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault<SyntaxNode>();
			var location = identifier?.GetLocation();

			syntaxContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1039_UseOfNamespaceReservedByAcumatica, location),
				pxContext.CodeAnalysisSettings);
		}
	}

	private bool IsAcumaticaNamespaceUsed(PXContext pxContext, SyntaxNode nodeToCheck)
	{
		if (nodeToCheck is NamespaceDeclarationSyntax namespaceDeclaration)
		{
			bool hasContainingNamespaces = namespaceDeclaration.Con
		}
		else if (nodeToCheck.RawKind == SharedConstants.FileScopedNamespaceDeclarationKind)
		{

		}
		else
			return false;
	}
}
