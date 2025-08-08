using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AddOrReplaceBaseDelegateParameterFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.Id,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.Id
			);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.PatchMethodName, out string? patchMethodName) ||
				patchMethodName.IsNullOrWhiteSpace())
			{
				patchMethodName = string.Empty;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			bool replaceLastParameter = diagnostic.IsFlagSet(PXOverrideDiagnosticProperties.DelegateParameterFixMode);
			context.CancellationToken.ThrowIfCancellationRequested();

			string? title = GetCodeFixTitle(diagnostic, patchMethodName);

			if (title == null)
				return Task.CompletedTask;

			var document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cToken => AddBaseDelegateParameterToPatchMethod(document, context.Span, patchMethodName, 
																							   replaceLastParameter, cToken),
											   equivalenceKey: nameof(Resources.PX1079Fix));
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static string? GetCodeFixTitle(Diagnostic diagnostic, string patchMethodName)
		{
			if (diagnostic.Id == Descriptors.PX1079_PXOverrideWithoutDelegateParameter.Id)
				return nameof(Resources.PX1079Fix).GetLocalized(patchMethodName).ToString();
			else if (diagnostic.Id == Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.Id)
				return nameof(Resources.PX1101Fix).GetLocalized().ToString();
			else
				return null;
		}

		private static async Task<Document> AddBaseDelegateParameterToPatchMethod(Document document, TextSpan span, string patchMethodName,
																				   bool replaceLastParameter, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var patchMethodNode = root?.FindNode(span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (patchMethodNode == null)
				return document;

			var patchMethodWithDelegateParameter = AddBaseDelegateParameter(patchMethodNode, patchMethodName, replaceLastParameter);

			if (patchMethodWithDelegateParameter == null)
				return document;

			cancellation.ThrowIfCancellationRequested();

			var newRoot = root!.ReplaceNode(patchMethodNode, patchMethodWithDelegateParameter);
			return document.WithSyntaxRoot(newRoot);
		}

		private static MethodDeclarationSyntax? AddBaseDelegateParameter(MethodDeclarationSyntax patchMethodNode, string patchMethodName, 
																		 bool replaceLastParameter)
		{
			string baseDelegateParameterName = CreateBaseDelegateParameterName(patchMethodName, patchMethodNode);
			var baseDelegateType = CreateBaseDelegateParameterType(patchMethodNode, replaceLastParameter);

			if (baseDelegateType == null)
				return null;

			var delegateParameter = Parameter(
										Identifier(baseDelegateParameterName))
									.WithType(baseDelegateType);

			if (replaceLastParameter)
			{
				var newParametersList = patchMethodNode.ParameterList.Parameters.RemoveAt(patchMethodNode.ParameterList.Parameters.Count - 1)
																				.Add(delegateParameter);
				return patchMethodNode.WithParameterList(
										ParameterList(newParametersList));
			}
			else
				return patchMethodNode.AddParameterListParameters(delegateParameter);
		}

		private static string CreateBaseDelegateParameterName(string patchMethodName, MethodDeclarationSyntax patchMethodNode)
		{
			patchMethodName = patchMethodName.NullIfWhiteSpace() ?? patchMethodNode.Identifier.Text;
			return !patchMethodName.IsNullOrWhiteSpace()
				? $"base_{patchMethodName}"
				: "baseDelegate";
		}

		private static TypeSyntax? CreateBaseDelegateParameterType(MethodDeclarationSyntax patchMethodNode, bool replaceLastParameter)
		{
			const int maxFuncAndActionParametersCount = 16; // Maximum number of parameters supported by Func and Action delegates is 16
			var parameters = patchMethodNode.ParameterList.Parameters;
			int parametersCountToUse = replaceLastParameter
				? parameters.Count - 1
				: parameters.Count;

			if (parametersCountToUse > maxFuncAndActionParametersCount)
				return null;

			bool useActionDelegate = patchMethodNode.IsVoidMethod();
			string delegateTypeName = useActionDelegate
				? nameof(Action)
				: nameof(Func<object>);

			if (useActionDelegate && parametersCountToUse == 0)
				return IdentifierName(delegateTypeName);        // Case of non-generic action delegate with no parameters

			var baseDelegateTypeParameters = GetBaseDelegateTypeParameters(patchMethodNode, parametersCountToUse, useActionDelegate);

			if (baseDelegateTypeParameters?.Count is null or 0)
				return null;

			var delegateTypeNameNode = 
				GenericName(
					Identifier(delegateTypeName),
					TypeArgumentList(
						SeparatedList(baseDelegateTypeParameters)
						)
					);

			return delegateTypeNameNode;
		}

		private static List<TypeSyntax>? GetBaseDelegateTypeParameters(MethodDeclarationSyntax patchMethodNode, int parametersCountToUse,
																	   bool useActionDelegate)
		{
			var parameters = patchMethodNode.ParameterList.Parameters;
			int estimatedCapacity = useActionDelegate ? parametersCountToUse : (parametersCountToUse + 1);
			var baseDelegateTypeParameters = new List<TypeSyntax>(estimatedCapacity);
			
			for (int i = 0; i < parametersCountToUse; i++)
			{
				if (parameters[i].Type is not TypeSyntax parameterType)
					return null;

				baseDelegateTypeParameters.Add(parameterType);
			}

			if (!useActionDelegate)
				baseDelegateTypeParameters.Add(patchMethodNode.ReturnType); // Need to add return type for Func delegate

			return baseDelegateTypeParameters;
		}
	}
}
