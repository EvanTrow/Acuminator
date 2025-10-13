using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MissingMandatoryDacFieldsFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new[]
			{
				Descriptors.PX1069_MissingSingleMandatoryDacField.Id,
				Descriptors.PX1069_MissingMultipleMandatoryDacFields.Id
			}
			.Distinct()
			.ToImmutableArray();

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var missingDacFieldInfos = GetMissingDacFieldInfos(diagnostic);

			if (missingDacFieldInfos?.Count is null or 0)
				return Task.CompletedTask;

			bool isSealedDac = diagnostic.IsFlagSet(PX1069Properties.IsSealedDac);
			context.CancellationToken.ThrowIfCancellationRequested();

			string codeActionName = nameof(Resources.PX1069Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => AddMissingDacFieldsAsync(context.Document, context.Span, missingDacFieldInfos,
																				  isSealedDac, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)>? GetMissingDacFieldInfos(Diagnostic diagnostic)
		{
			if (!diagnostic.TryGetPropertyValue(PX1069Properties.MissingMandatoryDacFieldsInfos, out string? missingDacFieldsInfos) ||
				missingDacFieldsInfos.IsNullOrWhiteSpace())
			{
				return null;
			}

			var dacFieldInfosStrings = missingDacFieldsInfos.Split(Constants.FieldKindsSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
			var dacFieldInfos = (from parsedInfo in dacFieldInfosStrings.Select(ParseDacFieldInfo)
								 where parsedInfo != null
								 select parsedInfo.Value
								).ToList(dacFieldInfosStrings.Length);
			return dacFieldInfos;
		}

		private static (DacFieldKind FieldKind, DacFieldInsertMode InsertMode)? ParseDacFieldInfo(string dacFieldInfoString)
		{
			if (dacFieldInfoString.IsNullOrWhiteSpace())
				return null;

			var parts = dacFieldInfoString.Split(Constants.FieldKindAndInsertModeSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
			
			if (parts.Length != 2)
				return null;

			var (fieldKindStr, insertModeStr) = (parts[0].NullIfWhiteSpace()?.Trim(), parts[1].NullIfWhiteSpace()?.Trim());

			if (fieldKindStr == null || !Enum.TryParse(fieldKindStr, ignoreCase: true, out DacFieldKind fieldKind))
				return null;

			if (insertModeStr == null || !Enum.TryParse(insertModeStr, ignoreCase: true, out DacFieldInsertMode insertMode))
				return null;

			return (fieldKind, insertMode);
		}

		private async Task<Document> AddMissingDacFieldsAsync(Document document, TextSpan span,
													List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)> missingDacFieldInfos, 
													bool isSealedDac, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? nodeWithDiagnostic = root?.FindNode(span);
			var dacNode = (nodeWithDiagnostic as ClassDeclarationSyntax) ??
						   nodeWithDiagnostic?.Parent<ClassDeclarationSyntax>();

			if (dacNode == null)
				return document;

			List<MemberDeclarationSyntax> newDacFieldNodes = GenerateMissingDacFieldNodes(missingDacFieldKinds, cancellationToken);

			if (newDacFieldNodes.Count == 0)
				return document;

			var newDacNode = dacNode.AddMembers(newDacFieldNodes.ToArray());
			var newRoot = root!.ReplaceNode(dacNode, newDacNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private List<MemberDeclarationSyntax> GenerateMissingDacFieldNodes(List<DacFieldKind> missingDacFieldKinds, CancellationToken cancellation)
		{
			List<MemberDeclarationSyntax> newDacFieldNodes = new(capacity: missingDacFieldKinds.Count * 2);

			foreach (DacFieldKind missingDacFieldKind in missingDacFieldKinds)
			{
				cancellation.ThrowIfCancellationRequested();

				var (fieldProperty, bqlField) = missingDacFieldKind switch
				{
					DacFieldKind.tstamp 				=> ,
					DacFieldKind.CreatedByID 			=> ,
					DacFieldKind.CreatedByScreenID 		=> ,
					DacFieldKind.CreatedDateTime 		=> ,
					DacFieldKind.LastModifiedByID 		=> ,
					DacFieldKind.LastModifiedByScreenID => ,
					DacFieldKind.LastModifiedDateTime 	=> ,
					_ 									=> null
				};

				if (fieldProperty != null && bqlField != null)
				{
					newDacFieldNodes.Add(bqlField);
					newDacFieldNodes.Add(fieldProperty);
				}
			}

			return newDacFieldNodes;
		}

		private SyntaxList<MemberDeclarationSyntax>? CreateMembersListWithBqlField(ClassDeclarationSyntax dacNode, 
																				   PropertyDeclarationSyntax propertyWithoutBqlFieldNode,
																				   string bqlFieldName, DataTypeName propertyDataType)
		{
			var members = dacNode.Members;

			if (members.Count == 0)
			{
				var newSingleBqlFieldNode = BqlFieldGeneration.GenerateTypedBqlField(propertyDataType, bqlFieldName, isFirstField: true, 
																					 isRedeclaration: false, propertyWithoutBqlFieldNode);
				return newSingleBqlFieldNode != null
					? SingletonList<MemberDeclarationSyntax>(newSingleBqlFieldNode)
					: null;
			}

			int propertyMemberIndex = dacNode.Members.IndexOf(propertyWithoutBqlFieldNode);

			if (propertyMemberIndex < 0)
				propertyMemberIndex = 0;

			var newBqlFieldNode = BqlFieldGeneration.GenerateTypedBqlField(propertyDataType, bqlFieldName, isFirstField: propertyMemberIndex == 0,
																		   isRedeclaration: false, propertyWithoutBqlFieldNode);
			if (newBqlFieldNode == null)
				return null;

			var propertyWithoutRegions = CodeGeneration.RemoveRegionsFromLeadingTrivia(propertyWithoutBqlFieldNode);
			var newMembers = members.Replace(propertyWithoutBqlFieldNode, propertyWithoutRegions)
									.Insert(propertyMemberIndex, newBqlFieldNode);
			return newMembers;
		}
	}
}