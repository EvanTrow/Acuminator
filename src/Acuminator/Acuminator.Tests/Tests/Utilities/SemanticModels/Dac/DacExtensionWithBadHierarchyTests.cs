#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Dac;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Dac
{
	public class DacExtensionWithBadHierarchyTests : SemanticModelTestsBase<DacSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("DacExtensionWithForbiddenHierarchy.cs")]
		public async Task DacExtension_WithForbiddenHierarchy(string text)
		{
			var testContext = await PrepareTestContextForCodeAsync(text).ConfigureAwait(false);

			var (inferredInfo, originalTypeSymbol) = GetInferredInfo(testContext, cancellation: default);

			CheckInferredInfoForBadHierarchy(inferredInfo);

			inferredInfo!.ExtensionWithBadBaseExtensions!.Should().Be(originalTypeSymbol);
		}

		private (InferredSymbolInfo? InferredInfo, INamedTypeSymbol OriginalTypeSymbol) GetInferredInfo(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration, cancellation);
			graphOrGraphExtDeclaration.Should().NotBeNull();

			var inferredInfo = DacAndDacExtInfoBuilder.Instance.InferTypeInfo(graphOrGraphExtSymbol!, context.PXContext, customDeclarationOrder: null,
																				  cancellation);
			return (inferredInfo, graphOrGraphExtSymbol!);
		}

		private void CheckInferredInfoForBadHierarchy(InferredSymbolInfo? inferredInfo)
		{
			inferredInfo.Should().NotBeNull();
			inferredInfo!.InferredInfo.Should().BeNull();
			inferredInfo.GetResultKind().Should().Be(InferResultKind.BadBaseExtensions);
			inferredInfo.ExtensionWithBadBaseExtensions.Should().NotBeNull();
		}

#pragma warning disable CS8609 // Nullability of reference types in return type doesn't match overridden member.
		protected override Task<DacSemanticModel?> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation) =>
			Task.FromResult<DacSemanticModel?>(null);
#pragma warning restore CS8609
	}
}
