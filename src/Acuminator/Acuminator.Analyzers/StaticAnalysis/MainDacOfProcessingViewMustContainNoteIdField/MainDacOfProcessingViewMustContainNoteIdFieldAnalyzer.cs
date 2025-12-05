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
				processingView.Symbol.GetSyntax(cancellation) is not SyntaxNode viewNode)
			{
				return processingView.Symbol.Locations.FirstOrDefault();
			}

			if (GetTypeNodeFromViewNode(viewNode) is not TypeSyntax viewTypeNode)
				return processingView.Symbol.Locations.FirstOrDefault();

			if (!processingView.Type.IsGenericType() || processingView.Type.TypeArguments().IsDefaultOrEmpty)
			{
				return viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
					   processingView.Symbol.Locations.FirstOrDefault();
			}

			return GetLocationFromViewTypeNode(viewTypeNode, processingView);
		}

		private static Location? GetLocationFromViewTypeNode(TypeSyntax viewTypeNode, DataViewInfo processingView)
		{
			int mainDacIndex = processingView.Type.TypeArguments().IndexOf(processingView.DAC!, SymbolEqualityComparer.Default);

			if (mainDacIndex < 0 || mainDacIndex >= processingView.Type.TypeArguments().Length)
			{
				return viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
					   processingView.Symbol.Locations.FirstOrDefault();
			}

			var genericTypeName = GetGenericNameFromTypeName(viewTypeNode);

			if (genericTypeName?.TypeArgumentList == null)
			{
				return viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
					   processingView.Symbol.Locations.FirstOrDefault();
			}

			var typeArgumentsNodes = genericTypeName.TypeArgumentList.Arguments;
			string dacName = processingView.DAC!.Name;
			string dacNameSuffix = $".{processingView.DAC!.Name}";

			if (mainDacIndex < typeArgumentsNodes.Count)
			{
				var mainDacTypeArgNode = typeArgumentsNodes[mainDacIndex];
				string mainDacTypeArgName = mainDacTypeArgNode.ToString();

				if (mainDacTypeArgName == dacName || mainDacTypeArgName.EndsWith(dacNameSuffix, StringComparison.Ordinal))
				{
					return mainDacTypeArgNode.GetLocation().NullIfLocationKindIsNone() ??
						   viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
						   processingView.Symbol.Locations.FirstOrDefault();
				}
			}

			var mainDacTypeArgFoundByDacName = GetMainDacTypeArgNode(typeArgumentsNodes, dacName, dacNameSuffix);
			return mainDacTypeArgFoundByDacName?.GetLocation().NullIfLocationKindIsNone() ??
				   viewTypeNode.GetLocation().NullIfLocationKindIsNone() ??
				   processingView.Symbol.Locations.FirstOrDefault();
		}

		private static TypeSyntax? GetTypeNodeFromViewNode(SyntaxNode viewNode) => viewNode switch
		{
			VariableDeclaratorSyntax viewVariableDeclarator   => viewVariableDeclarator.Parent<VariableDeclarationSyntax>()?.Type,
			PropertyDeclarationSyntax viewPropertyNode 		  => viewPropertyNode.Type,
			FieldDeclarationSyntax viewFieldNode 			  => viewFieldNode.Declaration?.Type,
			VariableDeclarationSyntax viewVariableDeclaration => viewVariableDeclaration.Type,
			_ 												  => null
		};

		private static GenericNameSyntax? GetGenericNameFromTypeName(TypeSyntax typeNode) =>
			typeNode switch
			{
				GenericNameSyntax genericTypeNameNode 	  => genericTypeNameNode,
				QualifiedNameSyntax qualifiedTypeNameNode => qualifiedTypeNameNode.Right as GenericNameSyntax,
				_ 										  => null
			};

		private static TypeSyntax? GetMainDacTypeArgNode(in SeparatedSyntaxList<TypeSyntax> typeArguments, string dacName,
														 string dacNameSuffix)
		{
			for (int i = 0; i < typeArguments.Count; i++)
			{
				var typeArg = typeArguments[i];
				string typeArgName = typeArg.ToString();

				if (typeArgName == dacName || typeArgName.EndsWith(dacNameSuffix, StringComparison.Ordinal))
				{
					return typeArg;
				}
			}

			return null;
		}
	}
}