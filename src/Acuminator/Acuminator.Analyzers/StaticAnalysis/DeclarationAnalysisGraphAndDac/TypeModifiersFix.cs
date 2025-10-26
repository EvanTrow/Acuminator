using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.DeclarationAnalysisGraphAndDac
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class TypeModifiersFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			new HashSet<string>
			{
				Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.Id,
				Descriptors.PX1113_SealedGraphsAndGraphExtensions.Id
			}
			.ToImmutableArray();

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			bool addAbstractModifier;
			string codeActionName;

			if (diagnostic.Id == Descriptors.PX1112_GenericGraphsAndGraphExtensionsMustBeAbstract.Id)
			{
				addAbstractModifier = true;
				codeActionName 		= nameof(Resources.PX1112Fix).GetLocalized().ToString();
			}
			else if (diagnostic.Id == Descriptors.PX1113_SealedGraphsAndGraphExtensions.Id)
			{
				addAbstractModifier = false;
				codeActionName 		= nameof(Resources.PX1113Fix).GetLocalized().ToString();
			}
			else
				return Task.CompletedTask;

			context.CancellationToken.ThrowIfCancellationRequested();

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => UpdateTypeModifiers(context.Document, context.Span, addAbstractModifier, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private async Task<Solution> UpdateTypeModifiers(Document document, TextSpan span, bool addAbstractModifier,
														 CancellationToken cancellationToken)
		{
			Solution originalSolution = document.Project.Solution;
			var (semanticModel, root) = await document.GetSemanticModelAndRootAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null || root == null)
				return originalSolution;

			SyntaxNode? diagnosticNode = root.FindNode(span);
			var graphOrDacOrExtensionToMakePublicNode = (diagnosticNode as ClassDeclarationSyntax) ?? diagnosticNode?.Parent<ClassDeclarationSyntax>();

			if (graphOrDacOrExtensionToMakePublicNode == null)
				return originalSolution;

			cancellationToken.ThrowIfCancellationRequested();

			var graphOrGraphExtTypeNode = semanticModel.GetDeclaredSymbol(graphOrDacOrExtensionToMakePublicNode, cancellationToken);

			if (graphOrGraphExtTypeNode == null)
				return originalSolution;

			var typeDeclarations = graphOrGraphExtTypeNode.DeclaringSyntaxReferences;

			if (typeDeclarations.Length <= 1)
			{
				var changedSolutionForNonPartialType = UpdateTypeNodeModifiersForNonPartialType(document, root, graphOrDacOrExtensionToMakePublicNode,
																								addAbstractModifier);
				return changedSolutionForNonPartialType;
			}
			else
			{
				var changedSolutionForPartialType = await UpdateTypeNodeModifiersForPartialType(document.Project.Solution, typeDeclarations,
																								addAbstractModifier, cancellationToken)
														.ConfigureAwait(false);
				return changedSolutionForPartialType;
			}	
		}

		private Solution UpdateTypeNodeModifiersForNonPartialType(Document document, SyntaxNode root, ClassDeclarationSyntax graphOrGraphExtTypeNode, 
																  bool addAbstractModifier)
		{
			var modifiedGraphOrGraphExtTypeNode = UpdateTypeNodeModifiers(graphOrGraphExtTypeNode, addAbstractModifier);

			if (ReferenceEquals(modifiedGraphOrGraphExtTypeNode, graphOrGraphExtTypeNode))
				return document.Project.Solution;

			var newRoot = root.ReplaceNode(graphOrGraphExtTypeNode, modifiedGraphOrGraphExtTypeNode);
			var changedDocument = document.WithSyntaxRoot(newRoot);

			return changedDocument.Project.Solution;
		}

		private async Task<Solution> UpdateTypeNodeModifiersForPartialType(Solution originalSolution, ImmutableArray<SyntaxReference> graphOrGraphExtDeclarations,
																		   bool addAbstractModifier, CancellationToken cancellation)
		{
			var solutionEditor = new SolutionEditor(originalSolution);
			var documentEditors = await GetAllDocumentEditorsAsync(solutionEditor, graphOrGraphExtDeclarations, cancellation).ConfigureAwait(false);

			foreach (DocumentEditor? documentEditor in documentEditors)
			{
				cancellation.ThrowIfCancellationRequested();

				if (documentEditor == null)
					continue;

				string path = documentEditor.OriginalDocument.FilePath.NullIfWhiteSpace() ?? documentEditor.OriginalDocument.Name;
				var graphOrGraphExtTypeNodesInDocument = GetTypeDeclarationsInDocument(path, graphOrGraphExtDeclarations, cancellation);

				foreach (var graphOrGraphExtTypeNode in graphOrGraphExtTypeNodesInDocument)
				{
					var modifiedGraphOrGraphExtTypeNode = UpdateTypeNodeModifiers(graphOrGraphExtTypeNode, addAbstractModifier);

					if (ReferenceEquals(modifiedGraphOrGraphExtTypeNode, graphOrGraphExtTypeNode))
						continue;

					documentEditor.ReplaceNode(graphOrGraphExtTypeNode, modifiedGraphOrGraphExtTypeNode);
				}
			}

			return solutionEditor.GetChangedSolution();
		}

		private Task<DocumentEditor?[]> GetAllDocumentEditorsAsync(SolutionEditor solutionEditor, ImmutableArray<SyntaxReference> typeDeclarations,
																  CancellationToken cancellation)
		{
			var declarationsByFile = typeDeclarations.GroupBy(typeDecl => typeDecl.SyntaxTree.FilePath);
			var documentEditorsTasks = new List<Task<DocumentEditor?>>(capacity: typeDeclarations.Length);

			foreach (var declarationsInFile in declarationsByFile)
			{
				cancellation.ThrowIfCancellationRequested();

				SyntaxTree? syntaxTree = declarationsInFile.FirstOrDefault()?.SyntaxTree;

				if (syntaxTree == null)
					continue;

				var documentId = solutionEditor.OriginalSolution.GetDocumentId(syntaxTree);

				if (documentId != null)
				{
					var documentEditorTask = solutionEditor.GetDocumentEditorAsync(documentId, cancellation);
					documentEditorsTasks.Add(documentEditorTask);
				}
			}

			return Task.WhenAll(documentEditorsTasks);
		}

		private IEnumerable<ClassDeclarationSyntax> GetTypeDeclarationsInDocument(string documentPath,
																				  ImmutableArray<SyntaxReference> typeDeclarations,
																				  CancellationToken cancellation)
		{
			return (from typeDecl in typeDeclarations
					where typeDecl.SyntaxTree.FilePath == documentPath
					select typeDecl.GetSyntax(cancellation))
				   .OfType<ClassDeclarationSyntax>();
		}

		private ClassDeclarationSyntax UpdateTypeNodeModifiers(ClassDeclarationSyntax graphOrGraphExtensionNode, bool addAbstractModifier)
		{
			var oldModifiers = graphOrGraphExtensionNode.Modifiers;
			SyntaxTokenList newModifiers = oldModifiers;
			bool modifiedAny = false;

			if (oldModifiers.Count > 0)
			{
				int sealedIndex = newModifiers.IndexOf(SyntaxKind.SealedKeyword);
				if (sealedIndex >= 0 && sealedIndex < newModifiers.Count)
				{
					newModifiers = newModifiers.RemoveAt(sealedIndex);
					modifiedAny = true;
				}
			}

			if (addAbstractModifier && !newModifiers.Any(SyntaxKind.AbstractKeyword) && !newModifiers.Any(SyntaxKind.SealedKeyword))
			{
				var abstractToken = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
				int partialModifierIndex = newModifiers.IndexOf(SyntaxKind.PartialKeyword);

				newModifiers = partialModifierIndex >= 0 && partialModifierIndex < newModifiers.Count 
									? newModifiers.Insert(partialModifierIndex, abstractToken)
									: newModifiers.Add(abstractToken);
				modifiedAny = true;
			}

			return modifiedAny
				? graphOrGraphExtensionNode.WithModifiers(newModifiers)
				: graphOrGraphExtensionNode;
		}
	}
}