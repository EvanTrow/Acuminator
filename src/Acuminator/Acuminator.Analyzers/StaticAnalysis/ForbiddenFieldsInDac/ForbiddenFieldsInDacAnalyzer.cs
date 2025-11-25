using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac
{
	/// <summary>
	/// A DAC declaration syntax analyzer.
	/// </summary>
	public class ForbiddenFieldsInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacFieldsForForbiddenNames(dacOrDacExtension, pxContext, context);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacFieldsForCompanyPrefix(dacOrDacExtension, pxContext, context);
		}
		
		private void CheckDacFieldsForForbiddenNames(DacSemanticModel dacOrDacExtension, PXContext pxContext, in SymbolAnalysisContext context)
		{
			var forbiddenNames = DacFieldNames.Restricted.All;

			foreach (string forbiddenFieldName in forbiddenNames)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (!dacOrDacExtension.DacFieldsByNames.TryGetValue(forbiddenFieldName, out var forbiddenDacField))
					continue;

				if (forbiddenDacField.PropertyInfo?.Node != null && forbiddenDacField.PropertyInfo.Symbol.IsDeclaredInType(dacOrDacExtension.Symbol))
				{
					RegisterForbiddenFieldDiagnosticForIdentifier(forbiddenDacField.PropertyInfo.Node.Identifier, pxContext, context);
				}

				if (forbiddenDacField.BqlFieldInfo?.Node != null && forbiddenDacField.BqlFieldInfo.Symbol.IsDeclaredInType(dacOrDacExtension.Symbol))
				{
					RegisterForbiddenFieldDiagnosticForIdentifier(forbiddenDacField.BqlFieldInfo.Node.Identifier, pxContext, context);
				}
			}
		}

		private void RegisterForbiddenFieldDiagnosticForIdentifier(SyntaxToken identifier, PXContext pxContext, in SymbolAnalysisContext context)
		{
			bool isDeletedDatabaseRecord = DacFieldNames.Restricted.DeletedDatabaseRecord.Equals(identifier.ValueText, StringComparison.OrdinalIgnoreCase);
			DiagnosticDescriptor descriptorToShow = 
				isDeletedDatabaseRecord && !pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
					? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV
					: Descriptors.PX1027_ForbiddenFieldsInDacDeclaration;

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(descriptorToShow, identifier.GetLocation(), identifier.ValueText), 
				pxContext.CodeAnalysisSettings);
		}

		private void CheckDacFieldsForCompanyPrefix(DacSemanticModel dacOrDacExtension, PXContext pxContext, in SymbolAnalysisContext context)
		{
			var dacFieldsWithCompanyPrefix = dacOrDacExtension.DeclaredDacFields
															  .Where(field => field.FieldCategory == DacFieldCategory.CompanyPrefix);

			foreach (DacFieldInfo companyPrefixedField in dacFieldsWithCompanyPrefix)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (companyPrefixedField.PropertyInfo?.Node != null && 
					companyPrefixedField.PropertyInfo.Symbol.IsDeclaredInType(dacOrDacExtension.Symbol))
				{
					RegisterCompanyPrefixDiagnosticForIdentifier(companyPrefixedField.PropertyInfo.Node.Identifier, pxContext, context);
				}

				if (companyPrefixedField.BqlFieldInfo?.Node != null && 
					companyPrefixedField.BqlFieldInfo.Symbol.IsDeclaredInType(dacOrDacExtension.Symbol))
				{
					RegisterCompanyPrefixDiagnosticForIdentifier(companyPrefixedField.BqlFieldInfo.Node.Identifier, pxContext, context);
				}
			}
		}

		private void RegisterCompanyPrefixDiagnosticForIdentifier(SyntaxToken identifier, PXContext pxContext, SymbolAnalysisContext context)
		{
			var diagnosticProperties = ImmutableDictionary<string, string?>.Empty
																		   .Add(DiagnosticProperty.RegisterCodeFix, bool.FalseString);
			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(
					Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName, identifier.GetLocation(), diagnosticProperties),
				pxContext.CodeAnalysisSettings);
		}
	}
}