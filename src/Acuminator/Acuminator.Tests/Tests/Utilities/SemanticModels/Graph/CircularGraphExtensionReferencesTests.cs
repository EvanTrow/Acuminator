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
		[EmbeddedFileData("GraphExtensionWIthCircularReference.cs")]
		public async Task SecondLevel_Derived_GraphExtension_InfoCollection(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);
			graphSemanticModel.Should().BeNull();
		}

#pragma warning disable CS8609 // Nullability of reference types in return type doesn't match overridden member.
		protected override Task<PXGraphSemanticModel?> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration);
			graphOrGraphExtSymbol.Should().NotBeNull();

			var graphSemanticModel = PXGraphSemanticModel.InferModel(context.PXContext, graphOrGraphExtSymbol!,
																	 GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																	 cancellation: cancellation);
			return Task.FromResult(graphSemanticModel);
		}
#pragma warning restore CS8609 // Nullability of reference types in return type doesn't match overridden member.
	}
}
