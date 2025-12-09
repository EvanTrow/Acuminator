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
	public class CircularDacExtensionReferencesTests : SemanticModelTestsBase<DacSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("DacExtensionWithCircularReference.cs")]
		public async Task DacExtension_WithCircularReference_Trivial(string text)
		{
			var testContext = await PrepareTestContextForCodeAsync(text).ConfigureAwait(false);

			var dacSemanticModel = await PrepareSemanticModelAsync(testContext, cancellation: default).ConfigureAwait(false);
			dacSemanticModel.Should().BeNull();
		}

#pragma warning disable CS8609 // Nullability of reference types in return type doesn't match overridden member.
		protected override Task<DacSemanticModel?> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation)
		{
			var dacOrDacExtDeclaration = context.Root.DescendantNodes()
													 .OfType<ClassDeclarationSyntax>()
													 .FirstOrDefault();
			dacOrDacExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? dacOrDacExtSymbol = context.SemanticModel.GetDeclaredSymbol(dacOrDacExtDeclaration, cancellation);
			dacOrDacExtDeclaration.Should().NotBeNull();

			var inferredInfo = DacAndDacExtInfoBuilder.Instance.InferTypeInfo(dacOrDacExtSymbol!, context.PXContext, customDeclarationOrder: null, 
																			  cancellation);
			inferredInfo.Should().NotBeNull();
			inferredInfo!.InferredInfo.Should().BeNull();
			inferredInfo.GetResultKind().Should().Be(InferResultKind.CircularReferences);
			inferredInfo.CircularReferenceExtension.Should().NotBeNull();
			inferredInfo.CircularReferenceExtension!.Should().Be(dacOrDacExtSymbol);

			return Task.FromResult<DacSemanticModel?>(null);
		}
#pragma warning restore CS8609
	}
}
