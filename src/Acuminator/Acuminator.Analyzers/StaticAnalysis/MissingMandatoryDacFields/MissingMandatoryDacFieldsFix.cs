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
using Acuminator.Utilities.Roslyn.CSharpVersion;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Acuminator.Utilities.Roslyn.Constants.TypeNames.SystemFieldsAttributes.ShortNames;
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

			var (semanticModel, root) = await document.GetSemanticModelAndRootAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null || root is not CompilationUnitSyntax compilationUnitNode)
				return document;

			SyntaxNode? nodeWithDiagnostic = compilationUnitNode.FindNode(span);
			var dacNode = (nodeWithDiagnostic as ClassDeclarationSyntax) ??
						   nodeWithDiagnostic?.Parent<ClassDeclarationSyntax>();

			if (dacNode == null)
				return document;

			var languageVersion = document.EffectiveCSharpVersion();
			var newDacFieldNodes = GenerateMissingDacFieldNodes(dacNode, semanticModel, missingDacFieldInfos, isSealedDac, 
																languageVersion, cancellationToken);
			if (newDacFieldNodes.Count == 0)
				return document;

			var newDacNode = InsertGeneratedFieldsIntoDac(dacNode, newDacFieldNodes);
			var newRoot = compilationUnitNode.ReplaceNode(dacNode, newDacNode);

			newRoot = newRoot.AddMissingUsingDirectiveForNamespace(NamespaceNames.System)
							 .AddMissingUsingDirectiveForNamespace(NamespaceNames.PXDataBql);
			return document.WithSyntaxRoot(newRoot);
		}

		private List<(GeneratedDacFieldNodeInfo FieldNodesInfo, DacFieldInsertMode InsertMode)> GenerateMissingDacFieldNodes(
																	ClassDeclarationSyntax dacNode, SemanticModel semanticModel,
																	List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)> missingDacFieldInfos,
																	bool isSealedDac, LanguageVersion? languageVersion, CancellationToken cancellation)
		{
			var dacMembers = dacNode.Members;
			var (isFirstInModifiedDac, indexInFieldInfos) = PredictInfoAboutFirstNewFieldInModifiedDac(missingDacFieldInfos, dacMembers);
			var newDacFieldNodes = new List<(GeneratedDacFieldNodeInfo FieldNodesInfo, DacFieldInsertMode InsertMode)>(missingDacFieldInfos.Count);

			for (int i = 0; i < missingDacFieldInfos.Count; i++)
			{
				cancellation.ThrowIfCancellationRequested();

				var (missingDacFieldKind, insertMode) = missingDacFieldInfos[i];
				bool isFirstField = isFirstInModifiedDac && i == indexInFieldInfos;
				var generatedPropertyAndField = GenerateMissingDacField(missingDacFieldKind, insertMode, semanticModel, isSealedDac, 
																		isFirstField, languageVersion, dacNode);
				if (generatedPropertyAndField == null)
					continue;

				newDacFieldNodes.Add((generatedPropertyAndField.Value, insertMode));
			}

			return newDacFieldNodes;
		}

		private static (bool IsFirstInModifiedDac, int IndexInFieldInfos) PredictInfoAboutFirstNewFieldInModifiedDac( 
																		List<(DacFieldKind FieldKind, DacFieldInsertMode InsertMode)> missingDacFieldInfos,
																		SyntaxList<MemberDeclarationSyntax> dacMembers)
		{
			// Try to predict which field will be first in the modified DAC and whether it will be a new DAC field
			bool isEmptyDac = dacMembers.Count == 0;
			bool isFirstFieldInModifiedDac = isEmptyDac;
			bool isFirstMemberCreatedAuditField;
			bool isFirstMemberLastModifiedAuditField;
			int firstNewFieldIndexInModifiedDac;

			InsertionOfFirstField();

			for (int i = 1; i < missingDacFieldInfos.Count; i++)
			{
				InsertionOfNonFirstField(i);
			}

			return (isFirstFieldInModifiedDac, firstNewFieldIndexInModifiedDac);

			//--------------------------------------------------Local Function------------------------------------------------------------
			void InsertionOfFirstField()
			{
				var firstFieldInfo = missingDacFieldInfos[0];

				if (!isEmptyDac)
				{
					firstNewFieldIndexInModifiedDac = -1;

					var firstDacMember = dacMembers[0];
					isFirstMemberCreatedAuditField = IsCreatedAuditField(firstDacMember);
					isFirstMemberLastModifiedAuditField = IsLastModifiedAuditField(firstDacMember);

					// For non-empty DAC use the regular iteration of the insertion prediction cycle
					InsertionOfNonFirstField(index: 0);
				}
				else
				{
					// For empty DAC the first field will always be in the beginning of the DAC no matter the insertion mode
					// isFirstFieldInModifiedDac is already true thanks to the isEmptyDac assignment above
					firstNewFieldIndexInModifiedDac = 0;

					isFirstMemberCreatedAuditField = firstFieldInfo.FieldKind.IsCreatedAuditField();
					isFirstMemberLastModifiedAuditField = !isFirstMemberCreatedAuditField && firstFieldInfo.FieldKind.IsLastModifiedAuditField();
				}
			}

			void InsertionOfNonFirstField(int index)
			{
				var (fieldKind, insertMode) = missingDacFieldInfos[index];

				if (insertMode == DacFieldInsertMode.AtTheBeginning ||
					(isFirstMemberCreatedAuditField && insertMode == DacFieldInsertMode.BeforeFirstCreatedAuditField) ||
					(isFirstMemberLastModifiedAuditField && insertMode == DacFieldInsertMode.BeforeFirstLastModifiedAuditField))
				{
					isFirstFieldInModifiedDac 			= true;
					firstNewFieldIndexInModifiedDac 	= index;
					isFirstMemberCreatedAuditField 		= fieldKind.IsCreatedAuditField();
					isFirstMemberLastModifiedAuditField = !isFirstMemberCreatedAuditField && fieldKind.IsLastModifiedAuditField();
				}
			}
		}

		private GeneratedDacFieldNodeInfo? GenerateMissingDacField(DacFieldKind missingDacFieldKind, DacFieldInsertMode insertMode,
																   SemanticModel semanticModel, bool isSealedDac, bool isFirstField, 
																   LanguageVersion? languageVersion, ClassDeclarationSyntax dacNode)
		{
			switch (missingDacFieldKind)
			{
				case DacFieldKind.tstamp:
				{
					bool isNullablePropertyType = IsNullableRefTypeForDacProperty(semanticModel, insertMode, dacNode);
					return GenerateTimestampField(isSealedDac, isFirstField, languageVersion, isNullablePropertyType);
				}
				case DacFieldKind.CreatedByID:
					return GenerateCreatedByIdField(isSealedDac, isFirstField, languageVersion);
				case DacFieldKind.CreatedByScreenID:
				{
					bool isNullablePropertyType = IsNullableRefTypeForDacProperty(semanticModel, insertMode, dacNode);
					return GenerateCreatedByScreenIdField(isSealedDac, isFirstField, languageVersion, isNullablePropertyType);
				}
				case DacFieldKind.CreatedDateTime:
					return GenerateCreatedDateTimeField(isSealedDac, isFirstField, languageVersion);
				case DacFieldKind.LastModifiedByID:
					return GenerateLastModifiedByIdField(isSealedDac, isFirstField, languageVersion);
				case DacFieldKind.LastModifiedByScreenID:
				{
					bool isNullablePropertyType = IsNullableRefTypeForDacProperty(semanticModel, insertMode, dacNode);
					return GenerateLastModifiedByScreenIdField(isSealedDac, isFirstField, languageVersion, isNullablePropertyType);
				}
				case DacFieldKind.LastModifiedDateTime:  
					return GenerateLastModifiedDateTimeField(isSealedDac, isFirstField, languageVersion);
				default:
					return null;
			}
		}

		private static bool IsNullableRefTypeForDacProperty(SemanticModel semanticModel, DacFieldInsertMode insertMode,
															ClassDeclarationSyntax dacNode)
		{
			var dacMembers = dacNode.Members;

			if (dacMembers.Count == 0)
			{
				NullableContext nullableContext = semanticModel.GetNullableContext(dacNode.SpanStart);
				return nullableContext.AreNullableAnnotationsEnabled();
			}

			switch (insertMode)
			{
				case DacFieldInsertMode.AtTheBeginning:
				{
					NullableContext nullableContext = semanticModel.GetNullableContext(dacMembers[0].SpanStart);
					return nullableContext.AreNullableAnnotationsEnabled();
				}
				case DacFieldInsertMode.BeforeFirstCreatedAuditField:
				case DacFieldInsertMode.AfterLastCreatedAuditField:
				case DacFieldInsertMode.BeforeFirstLastModifiedAuditField:
				case DacFieldInsertMode.AfterLastLastModifiedAuditField:
				{
					int memberIndex = insertMode switch
					{
						DacFieldInsertMode.BeforeFirstCreatedAuditField 	 => dacMembers.IndexOf(IsCreatedAuditField),
						DacFieldInsertMode.AfterLastCreatedAuditField 		 => dacMembers.LastIndexOf(IsCreatedAuditField) + 1,
						DacFieldInsertMode.BeforeFirstLastModifiedAuditField => dacMembers.IndexOf(IsLastModifiedAuditField),
						DacFieldInsertMode.AfterLastLastModifiedAuditField   => dacMembers.LastIndexOf(IsLastModifiedAuditField) + 1,
						_ 													 => -1
					}; 

					int position = memberIndex >= 0 && memberIndex < dacMembers.Count
						? dacMembers[memberIndex].SpanStart 
						: dacMembers[^1].SpanStart;
					NullableContext nullableContext = semanticModel.GetNullableContext(position);
					return nullableContext.AreNullableAnnotationsEnabled();
				}
				case DacFieldInsertMode.AtTheEnd:
				default:
				{
					NullableContext nullableContext = semanticModel.GetNullableContext(dacMembers[^1].SpanStart);
					return nullableContext.AreNullableAnnotationsEnabled();
				}
			}
		}

		private GeneratedDacFieldNodeInfo? GenerateTimestampField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion,
																  bool isNullablePropertyType) =>
			GenerateDacField(DacFieldNames.System.Timestamp, TypeNames.ByteArray, isNullablePropertyType,
							 PXDBTimestamp, isSealedDac, isFirstField, languageVersion);
		
		private GeneratedDacFieldNodeInfo? GenerateCreatedByIdField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion) =>
			GenerateDacField(DacFieldNames.System.CreatedByID, nameof(Guid), isNullablePropertyType: true,
							 PXDBCreatedByID, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateLastModifiedByIdField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion) =>
			GenerateDacField(DacFieldNames.System.LastModifiedByID, nameof(Guid), isNullablePropertyType: true,
							 PXDBLastModifiedByID, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateCreatedByScreenIdField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion,
																		  bool isNullablePropertyType) =>
			GenerateDacField(DacFieldNames.System.CreatedByScreenID, TypeNames.CSharpPredefinedTypes.String, isNullablePropertyType,
							 PXDBCreatedByScreenID, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateLastModifiedByScreenIdField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion,
																			   bool isNullablePropertyType) =>
			GenerateDacField(DacFieldNames.System.LastModifiedByScreenID, TypeNames.CSharpPredefinedTypes.String, isNullablePropertyType,
							 PXDBLastModifiedByScreenID, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateCreatedDateTimeField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion) =>
			GenerateDacField(DacFieldNames.System.CreatedDateTime, nameof(DateTime), isNullablePropertyType: true,
							 PXDBCreatedDateTime, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateLastModifiedDateTimeField(bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion) =>
			GenerateDacField(DacFieldNames.System.LastModifiedDateTime, nameof(DateTime), isNullablePropertyType: true,
							 PXDBLastModifiedDateTime, isSealedDac, isFirstField, languageVersion);

		private GeneratedDacFieldNodeInfo? GenerateDacField(string dacFieldName, string propertyTypeName, bool isNullablePropertyType,
															string attributeTypeName, bool isSealedDac, bool isFirstField, LanguageVersion? languageVersion)
		{
			var propertyType = new DataTypeName(propertyTypeName);
			var fieldGenerationOptions = new DacFieldGenerationOptions(propertyType, isNullablePropertyType, isSealedDac,
																		isFirstField, languageVersion);
			AttributeListSyntax[] attributeLists =
			[
				AttributeList(
					SingletonSeparatedList(
						Attribute(IdentifierName(attributeTypeName))))
			];

			var generatedField = DacFieldGeneration.GenerateDacField(dacFieldName, fieldGenerationOptions, attributeLists);
			return generatedField;
		}

		private ClassDeclarationSyntax InsertGeneratedFieldsIntoDac(ClassDeclarationSyntax dacNode,
													List<(GeneratedDacFieldNodeInfo FieldNodesInfo, DacFieldInsertMode InsertMode)> newDacFieldNodes)
		{
			if (dacNode.Members.Count == 0)
			{
				var newFieldNodes = newDacFieldNodes.SelectMany(m => m.FieldNodesInfo.GetNodes());
				return dacNode.WithMembers(
								List(newFieldNodes));
			}

			var newDacMembers = dacNode.Members.ToList(2 * newDacFieldNodes.Count + dacNode.Members.Count);

			foreach (var (newDacFieldInfo, insertMode) in newDacFieldNodes)
			{
				var newFieldNodes = newDacFieldInfo.GetNodes();
				int indexToInsert = insertMode switch
				{
					DacFieldInsertMode.AtTheBeginning 					 => 0,
					DacFieldInsertMode.AtTheEnd		  					 => -1,
					DacFieldInsertMode.BeforeFirstCreatedAuditField 	 => newDacMembers.FindIndex(IsCreatedAuditField),
					DacFieldInsertMode.AfterLastCreatedAuditField 		 => newDacMembers.FindLastIndex(IsCreatedAuditField) + 1,
					DacFieldInsertMode.BeforeFirstLastModifiedAuditField => newDacMembers.FindIndex(IsLastModifiedAuditField),
					DacFieldInsertMode.AfterLastLastModifiedAuditField 	 => newDacMembers.FindLastIndex(IsLastModifiedAuditField) + 1,
					_ 													 => -1
				};

				if (indexToInsert < 0 || indexToInsert >= newDacMembers.Count)
					newDacMembers.AddRange(newFieldNodes);
				else
					newDacMembers.InsertRange(indexToInsert, newFieldNodes);
			}

			var newDacNode = dacNode.WithMembers(
										List(newDacMembers));
			return newDacNode;
		}

		private static bool IsCreatedAuditField(MemberDeclarationSyntax memberNode) =>
			NodeBelongsToAuditFieldsSet(memberNode, useCreatedAuditFieldsSet: true);

		private static bool IsLastModifiedAuditField(MemberDeclarationSyntax memberNode) =>
			NodeBelongsToAuditFieldsSet(memberNode, useCreatedAuditFieldsSet: false);

		private static bool NodeBelongsToAuditFieldsSet(MemberDeclarationSyntax memberNode, bool useCreatedAuditFieldsSet)
		{
			string? memberName = memberNode switch
			{
				PropertyDeclarationSyntax propertyNode => propertyNode.Identifier.Text,
				ClassDeclarationSyntax bqlFieldNode    => bqlFieldNode.Identifier.Text,
				_ 									   => null
			};

			if (memberName == null)
				return false;

			if (useCreatedAuditFieldsSet)
			{
				return string.Equals(memberName, DacFieldNames.System.CreatedByID, StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(memberName, DacFieldNames.System.CreatedByScreenID, StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(memberName, DacFieldNames.System.CreatedDateTime, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return string.Equals(memberName, DacFieldNames.System.LastModifiedByID, StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(memberName, DacFieldNames.System.LastModifiedByScreenID, StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(memberName, DacFieldNames.System.LastModifiedDateTime, StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}