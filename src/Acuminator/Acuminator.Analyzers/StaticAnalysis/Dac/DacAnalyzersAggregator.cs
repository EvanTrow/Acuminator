using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacFieldAndReferencedFieldMismatch;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisDac;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;
using Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;
using Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute;
using Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacAnalyzersAggregator : SymbolAnalyzersAggregator<IDacAnalyzer>
	{
		protected override SymbolKind SymbolKind => SymbolKind.NamedType;

		public DacAnalyzersAggregator() : this(null,
			new DacAndDacExtensionDeclarationAnalyzer(),
			new DacPropertyAttributesAnalyzer(),
			new DacAutoNumberAttributeAnalyzer(),
			new DacNonAbstractFieldTypeAnalyzer(),
			new UnderscoresInDacAnalyzer(),
			new NonPublicGraphAndDacAndExtensionsAnalyzer(),
			new ForbiddenFieldsInDacAnalyzer(),
			new NoBqlFieldForDacFieldPropertyAnalyzer(),
			new MissingBqlFieldRedeclarationInDerivedDacAnalyzer(),
			new MissingMandatoryDacFieldsAnalyzer(),
			new PropertyAndBqlFieldTypesMismatchAnalyzer(),
			new LegacyBqlFieldAnalyzer(),
			new MethodsUsageInDacAnalyzer(),
			new KeyFieldDeclarationAnalyzer(),
			new DacPrimaryAndUniqueKeyDeclarationAnalyzer(),
			new DacForeignKeyDeclarationAnalyzer(),
			new DacExtensionDefaultAttributeAnalyzer(),
			new NonNullableTypeForBqlFieldAnalyzer(),
			new MissingTypeListAttributeAnalyzer(),
			new PXGraphUsageInDacAnalyzer(),
			new NoIsActiveMethodForExtensionAnalyzer(),
			new PXGraphCreationInGraphInWrongPlacesDacAnalyzer(),
			new DacFieldAndReferencedFieldMismatchAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public DacAnalyzersAggregator(CodeAnalysisSettings? settings, params IDacAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
		{
		}

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not INamedTypeSymbol type)
				return;

			var dacOrDacExtInfo = InferSymbolInfo(context, pxContext, type);

			if (dacOrDacExtInfo == null)
				return;

			var inferredDacModel = DacSemanticModel.InferModel(pxContext, dacOrDacExtInfo, cancellation: context.CancellationToken);

			if (inferredDacModel == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();
			var effectiveDacAnalyzers = InnerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, inferredDacModel))
													  .ToList(capacity: InnerAnalyzers.Length);

			RunAggregatedAnalyzersInParallel(effectiveDacAnalyzers, context, analyzerIndex =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var aggregatedAnalyzer = effectiveDacAnalyzers[analyzerIndex];
				aggregatedAnalyzer.Analyze(context, pxContext, inferredDacModel);
			});
		}

		private DacOrDacExtInfoBase? InferSymbolInfo(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type)
		{
			InferredSymbolInfo? inferredInfo = DacAndDacExtInfoBuilder.Instance.InferTypeInfo(type, pxContext, customDeclarationOrder: null, 
																							  context.CancellationToken);
			if (inferredInfo == null)
				return null;

			InferResultKind resultKind = inferredInfo.GetResultKind();

			switch (resultKind)
			{
				case InferResultKind.MultipleRootTypes:
					ReportMultipleRoots(context, pxContext, type, inferredInfo.CollectedRootTypes);
					return null;
				case InferResultKind.CircularReferences:
					ReportCircularExtensions(context, pxContext, type, inferredInfo.CircularReferenceExtension!);
					return null;

				case InferResultKind.BadBaseExtensions:
					ReportBadBaseExtensions(context, pxContext, type, inferredInfo.ExtensionWithBadBaseExtensions!);
					return null;

				case InferResultKind.Success:
					return inferredInfo.InferredInfo as DacOrDacExtInfoBase;

				// For unknown errors we do not report anything
				case InferResultKind.UnrecognizedError:
				default:
					return null;
			}
		}

		private void ReportMultipleRoots(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type,
										 IReadOnlyCollection<ITypeSymbol> multipleRootTypes)
		{
			// TODO implement diagnostic for multiple root types
		}

		private void ReportCircularExtensions(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type, ITypeSymbol circularExtension)
		{
			// TODO implement diagnostic for circular extensions
		}

		private void ReportBadBaseExtensions(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol type, 
											 ITypeSymbol extensionWithBadBaseExtensions)
		{
			// TODO implement diagnostic for DAC extensions with bad base extensions
		}
	}
}
