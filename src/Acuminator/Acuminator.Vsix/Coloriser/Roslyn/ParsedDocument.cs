#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

using Path = System.IO.Path;

namespace Acuminator.Vsix.Coloriser
{
	public class ParsedDocument(Workspace workspace, Document document, SyntaxNode syntaxRoot, SemanticModel semanticModel, 
								ITextSnapshot snapshot)
	{
		private static readonly HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
		   Constants.CSharp.FileExtension
		};

		public Workspace Workspace { get; } = workspace.CheckIfNull();

		public Document Document { get; } = document.CheckIfNull();

		public SyntaxNode SyntaxRoot { get; } = syntaxRoot.CheckIfNull();

		public SemanticModel SemanticModel { get; } = semanticModel.CheckIfNull();

		public ITextSnapshot Snapshot { get; } = snapshot.CheckIfNull();

		public static async Task<ParsedDocument?> ResolveAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return null;

			Workspace? workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync()
																	 .ConfigureAwait(false);
			Document? document = snapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (workspace == null || document == null || !IsSupportedFileType(document) || !document.SupportsSemanticModel ||
				!document.SupportsSyntaxTree)
			{
				return null;        // Razor cshtml returns a null document for some reason.
			}
			
			var semanticModel = await GetSemanticModelAsync(document, cancellationToken).ConfigureAwait(false);

			if (cancellationToken.IsCancellationRequested || semanticModel is null)
				return null;

			var syntaxRoot = await GetSyntaxRootAsync(document, cancellationToken).ConfigureAwait(false);

			if (cancellationToken.IsCancellationRequested || syntaxRoot is null)
				return null;

			return new ParsedDocument(workspace, document, syntaxRoot, semanticModel, snapshot);
		}

		private static async ValueTask<SemanticModel?> GetSemanticModelAsync(Document document, CancellationToken cancellationToken)
		{
			if (document.TryGetSemanticModel(out SemanticModel? semanticModel))
				return semanticModel;

			var semanticModelTaskResult = await document.GetSemanticModelAsync(cancellationToken)
														.TryAwait()
														.ConfigureAwait(false);
			semanticModel = semanticModelTaskResult.Result;
			return semanticModelTaskResult.IsSuccess ? semanticModel : null;
		}

		private static async ValueTask<SyntaxNode?> GetSyntaxRootAsync(Document document, CancellationToken cancellationToken)
		{
			if (document.TryGetSyntaxRoot(out SyntaxNode? syntaxRoot))
				return syntaxRoot;

			var syntaxRootTaskResult = await document.GetSyntaxRootAsync(cancellationToken)
													 .TryAwait()
													 .ConfigureAwait(false);
			syntaxRoot = syntaxRootTaskResult.Result;
			return syntaxRootTaskResult.IsSuccess ? syntaxRoot : null;
		}

		private static bool IsSupportedFileType(Document document) => allowedExtensions.Contains(Path.GetExtension(document.FilePath));
	}
}
