using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		public LongOperationDelegateClosuresAnalyzer() : this(null)
		{ }

		public LongOperationDelegateClosuresAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeLongOperationDelegates(c, pxContext), SyntaxKind.CompilationUnit);
		}

		private static void AnalyzeLongOperationDelegates(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not CompilationUnitSyntax rootNode)
				return;

			var longOperationsChecker = new LongOperationsChecker(syntaxContext, pxContext);
			var typeDeclarations = rootNode.DescendantNodes()
										   .OfType<TypeDeclarationSyntax>();
	
			foreach (TypeDeclarationSyntax typeDeclaration in typeDeclarations)
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				var typeMembers = typeDeclaration.Members;

				if (typeMembers.Count > 0)
				{
					longOperationsChecker.CheckForCapturedGraphReferencesInDelegateClosures(typeDeclaration);
				}
			}
		}
	}
}