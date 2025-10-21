using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public static class SuppressionExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanNodeContainSuppressionComment(SyntaxNode node) =>
			node is StatementSyntax or MemberDeclarationSyntax or UsingDirectiveSyntax or ArgumentSyntax or ParameterSyntax;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldStopSearchForSuppressionComment(SyntaxNode node) =>
			node is StatementSyntax or MemberDeclarationSyntax or UsingDirectiveSyntax;

		public static void ReportDiagnosticWithSuppressionCheck(this SymbolAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			if (diagnostic.Location.SourceTree == null)
				return;

			var semanticModel = context.Compilation.GetSemanticModel(diagnostic.Location.SourceTree);

			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				semanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this SyntaxNodeAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this CodeBlockAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}

		public static string GetXDocumentStringWithDeclaration(this XDocument xDocument)
		{
			xDocument.ThrowOnNull();

			var builder = new StringBuilder(capacity: 65);

			using (TextWriter writer = new Utf8StringWriter(builder))
			{
				xDocument.Save(writer);
			}

			return builder.ToString();
		}

		public static bool IsSuppressionFile([NotNullWhen(returnValue: true)] this string? filePath, bool checkFileExists)
		{
			if (filePath.IsNullOrWhiteSpace() || (checkFileExists && !File.Exists(filePath)))
				return false;

			string fileExtension = Path.GetExtension(filePath);
			return SuppressionFile.SuppressionFileExtension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase);
		}
	}
}
