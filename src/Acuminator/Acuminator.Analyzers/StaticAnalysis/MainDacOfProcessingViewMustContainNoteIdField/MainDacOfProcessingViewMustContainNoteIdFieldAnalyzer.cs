using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MainDacOfProcessingViewMustContainNoteIdField
{
	public class MainDacOfProcessingViewMustContainNoteIdFieldAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create(Descriptors.PX1111_MainDacOfProcessingViewMustContainNoteIdField);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExt) =>
			base.ShouldAnalyze(pxContext, graphOrGraphExt) && graphOrGraphExt.IsProcessing;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var processingViews = graphOrGraphExt.DeclaredViews.Where(view => view.IsProcessing && view.DAC != null);

			foreach (DataViewInfo view in processingViews)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				CheckMainDacOfProcessingView(context, pxContext, view);
			}
		}

		private void CheckMainDacOfProcessingView(SymbolAnalysisContext context, PXContext pxContext, DataViewInfo processingView)
		{
			// DAC is not null which is checked by the caller
			var dacWithBaseDacTypes = processingView.DAC!.GetDacWithBaseTypesThatMayStoreDacProperties(pxContext);
			var pxNoteAttribute		= pxContext.AttributeTypes.PXNoteAttribute;

			bool hasNoteIdField = 
				dacWithBaseDacTypes.SelectMany(type => type.GetProperties())
								   .Any(property => DacFieldNames.System.NoteID.Equals(property.Name, StringComparison.OrdinalIgnoreCase) &&
													property.HasAttribute(pxNoteAttribute, checkOverrides: false, checkForDerivedAttributes: true));
			if (hasNoteIdField)
				return;	
		
			Location? location = GetLocation(processingView, context.CancellationToken);
			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1111_MainDacOfProcessingViewMustContainNoteIdField, location,
								  processingView.DAC!.Name, processingView.Name),
				pxContext.CodeAnalysisSettings);
		}

		private static Location? GetLocation(DataViewInfo processingView, CancellationToken cancellation)
		{
			if (!processingView.Symbol.IsInSourceCode() ||
				!processingView.Type.IsGenericType || processingView.Type.TypeArguments.IsDefaultOrEmpty)
			{
				return processingView.Symbol.Locations.FirstOrDefault();
			}

			return processingView.DAC?.Locations.FirstOrDefault() ??
				   processingView.Symbol.Locations.FirstOrDefault();

			//if ()
			//	return processingView.Symbol.Locations.FirstOrDefault();

			//var location = processingView.Type.IsGenericType && !
			//	? processingView.Symbol.Locations.FirstOrDefault()
			//	: GetTypeNodeFromViewNode(viewNode)?.GetLocation().NullIfLocationKindIsNone()
			//	  ?? processingView.Symbol.Locations.FirstOrDefault();

			//var viewTypeNode = GetTypeNodeFromViewNode(viewNode);

			//if (viewTypeNode == null)
			//	return processingView.Symbol.Locations.FirstOrDefault();
			//else if (!processingView.Type.IsGenericType || processingView.Type.TypeArguments.IsDefaultOrEmpty)
			//{
			//	return viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
			//			processingView.Symbol.Locations.FirstOrDefault();
			//}
		}

		private static TypeSyntax? GetTypeNodeFromViewNode(SyntaxNode viewNode) => viewNode switch
		{
			PropertyDeclarationSyntax viewPropertyNode 		  => viewPropertyNode.Type,
			FieldDeclarationSyntax viewFieldNode 			  => viewFieldNode.Declaration?.Type,
			VariableDeclarationSyntax viewVariableDeclaration => viewVariableDeclaration.Type,
			VariableDeclaratorSyntax viewVariableDeclarator   => viewVariableDeclarator.Parent<VariableDeclarationSyntax>()?.Type,
			_ 												  => null
		};
	}
}