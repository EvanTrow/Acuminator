using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Resources;

namespace Acuminator.Runner.Output.PlainText
{
	/// <summary>
	/// The report outputter to console in the plain text format.
	/// </summary>
	internal class PlainTextReportOutputterConsole : PlainTextReportOutputterBase
	{
		public override void Dispose() { }

		protected override void WriteTitle(in Title? title, int indentationLevel, int diagnosticsCount, int? distinctDiagnosticsCount, bool hasContent)
		{
			if (title == null)
				return;

			string padding = GetPadding(indentationLevel);
			string suffix = hasContent ? ":" : string.Empty;
			string titleWithPadding = distinctDiagnosticsCount.HasValue
				? $"{padding}{title.Value.Text}({Messages.ErrorsCountReportTitlePart}: {diagnosticsCount}, " + 
					$"{Messages.DistinctDiagnosticsReportTitlePart}: {distinctDiagnosticsCount.Value}){suffix}"
				: $"{padding}{title.Value.Text}({Messages.ErrorsCountReportTitlePart}: {diagnosticsCount}){suffix}";

			switch (title?.Kind)
			{
				case TitleKind.File:
					WriteFileName(titleWithPadding);
					return;
				case TitleKind.DiagnosticId:
					WriteDiagnosticIdTitle(titleWithPadding);
					return;
				case TitleKind.AllDiagnostics:
					WriteAllDiagnosticsTitle(titleWithPadding);
					return;
				default:
					WriteLine(titleWithPadding);
					return;
			}
		}

		protected override void WriteLine() => Console.WriteLine();

		protected override void WriteLine(string text) => Console.WriteLine(text);

		protected override void WriteLine(in Line line, int indentationLevel)
		{
			switch (line.Spans.Length)
			{
				case 0:
					WriteLine();
					return;

				case 2:
					WriteDiagnosticWithLocation(indentationLevel, diagnosticContent: line.Spans[0].ToString(), 
												location: line.Spans[1].ToString());
					return;
					
				case 1:
				default:
					string padding = GetPadding(indentationLevel);
					WriteLine(padding + line.ToString());
					return;
			}
		}

		private void WriteDiagnosticWithLocation(int indentationLevel, string diagnosticContent, string location)
		{ 
			string padding = GetPadding(indentationLevel);
			var oldColor   = Console.ForegroundColor;

			try
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(padding + diagnosticContent);
			}
			finally
			{
				Console.ForegroundColor = oldColor;
			}

			Console.Write($": {location}{Environment.NewLine}");
		}

		private void WriteAllDiagnosticsTitle(string allDiagnosticsTitle) =>
			OutputTitle(allDiagnosticsTitle, ConsoleColor.DarkCyan);

		private void WriteFileName(string fileName) =>
			OutputTitle(fileName, ConsoleColor.Magenta);

		private void WriteDiagnosticIdTitle(string diagnosticIdTitle) =>
			 OutputTitle(diagnosticIdTitle, ConsoleColor.Green);

		private void OutputTitle(string text, ConsoleColor color)
		{
			var oldColor = Console.ForegroundColor;

			try
			{
				Console.ForegroundColor = color;
				Console.WriteLine(text);
			}
			finally
			{
				Console.ForegroundColor = oldColor;
			}
		}
	}
}
