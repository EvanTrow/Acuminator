#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Graph;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph
{
	public class CircularGraphExtensionReferencesTests : SemanticModelTestsBase<PXGraphSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("GraphExtensionWithTrivialCircularReference.cs")]
		public async Task GraphExtension_WithCircularReference_Trivial(string text)
		{
			var testContext = await PrepareTestContextForCodeAsync(text).ConfigureAwait(false);

			var (inferredInfo, originalTypeSymbol) = GetInferredInfo(testContext, cancellation: default);

			CheckInferredInfoForCircularReference(inferredInfo);

			inferredInfo!.CircularReferenceExtension!.Should().Be(originalTypeSymbol);
		}

		[Theory]
		[EmbeddedFileData("GraphExtensionWithComplexCircularReference.cs")]
		public async Task GraphExtension_WithCircularReference_Complex(string text)
		{
			var testContext = await PrepareTestContextForCodeAsync(text).ConfigureAwait(false);

			var (inferredInfo, originalTypeSymbol) = GetInferredInfo(testContext, cancellation: default);

			CheckInferredInfoForCircularReference(inferredInfo);

			inferredInfo!.CircularReferenceExtension!.Should().Be(originalTypeSymbol);
		}

		private (InferredSymbolInfo? InferrefInfo, INamedTypeSymbol OriginalTypeSymbol) GetInferredInfo(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration, cancellation);
			graphOrGraphExtDeclaration.Should().NotBeNull();

			var inferredInfo = GraphAndGraphExtInfoBuilder.Instance.InferTypeInfo(graphOrGraphExtSymbol!, context.PXContext, customDeclarationOrder: null,
																				  cancellation);
			return (inferredInfo, graphOrGraphExtSymbol!);
		}

		private void CheckInferredInfoForCircularReference(InferredSymbolInfo? inferredInfo)
		{
			inferredInfo.Should().NotBeNull();
			inferredInfo!.InferredInfo.Should().BeNull();
			inferredInfo.GetResultKind().Should().Be(InferResultKind.CircularReferences);
			inferredInfo.CircularReferenceExtension.Should().NotBeNull();
		}

#pragma warning disable CS8609 // Nullability of reference types in return type doesn't match overridden member.
		protected override Task<PXGraphSemanticModel?> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation) =>
			Task.FromResult<PXGraphSemanticModel?>(null);
#pragma warning restore CS8609 // Nullability of reference types in return type doesn't match overridden member.
	}
}
