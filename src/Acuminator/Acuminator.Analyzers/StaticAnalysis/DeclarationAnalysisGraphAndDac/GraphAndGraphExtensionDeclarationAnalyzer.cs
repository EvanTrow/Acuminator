using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraphAndDac
{
	public class GraphAndGraphExtensionDeclarationAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			CheckIfGraphOrGraphExtensionIsGenericNonAbstract(context, pxContext, graphOrGraphExt);
		}

		protected virtual void CheckIfGraphOrGraphExtensionIsGenericNonAbstract(SymbolAnalysisContext context, PXContext pxContext,
																				PXGraphEventSemanticModel graphOrGraphExt)
		{
			if (graphOrGraphExt.Symbol.IsAbstract || !graphOrGraphExt.Symbol.IsGenericType ||
				graphOrGraphExt.Symbol.TypeParameters.IsDefaultOrEmpty || graphOrGraphExt.Node == null)
			{
				return;
			}
			
			var location = graphOrGraphExt.Node.Identifier.GetLocation().NullIfLocationKindIsNone() ??
						   graphOrGraphExt.Node.GetLocation();
			var diagnostic = Diagnostic.Create(Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract, location,
												graphOrGraphExt.Symbol.Name);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}