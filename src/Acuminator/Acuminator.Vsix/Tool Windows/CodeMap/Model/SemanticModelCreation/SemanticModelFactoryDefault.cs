#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Default implementation for <see cref="ISemanticModelFactory"/> interface.
	/// </summary>
	public class SemanticModelFactoryDefault : ISemanticModelFactory
	{
		/// <summary>
		/// Try to infer semantic model for <paramref name="rootSymbol"/>. If semantic model can't be inferred
		/// the <paramref name="semanticModel"/> is null and the method returns false.
		/// </summary>
		/// <param name="rootSymbol">The root symbol.</param>
		/// <param name="context">The context.</param>
		/// <param name="semanticModel">[out] The inferred semantic model.</param>
		/// <param name="declarationOrder">(Optional) The declaration order of the <see cref="ISemanticModel.Symbol"/>.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns>
		/// True if it succeeds, false if it fails.
		/// </returns>
		public virtual bool TryToInferSemanticModel(ITypeSymbol rootSymbol, PXContext context, out ISemanticModel? semanticModel,
													int? declarationOrder = null, CancellationToken cancellationToken = default)
		{
			rootSymbol.ThrowOnNull();
			context.ThrowOnNull();
			cancellationToken.ThrowIfCancellationRequested();

			if (rootSymbol.IsPXGraphOrExtension(context))
			{
				return TryToInferGraphOrGraphExtensionSemanticModel(rootSymbol, context, declarationOrder, 
																	GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																	out semanticModel, cancellationToken);
			}
			else if (rootSymbol.IsDacOrExtension(context))
			{
				return TryToInferDacOrDacExtensionSemanticModel(rootSymbol, context, declarationOrder, out semanticModel, cancellationToken);
			}

			semanticModel = null;
			return false;
		}

		protected virtual bool TryToInferGraphOrGraphExtensionSemanticModel(ITypeSymbol graphSymbol, PXContext context, int? declarationOrder,
																			GraphSemanticModelCreationOptions modelCreationOptions,
																			out ISemanticModel? graphModelForCodeMap,
																			CancellationToken cancellationToken = default)
		{
			var graphModelWithEvents = PXGraphEventSemanticModel.InferModel(context, graphSymbol, modelCreationOptions,
																			declarationOrder, cancellationToken);
			if (graphModelWithEvents == null)
			{
				graphModelForCodeMap = null;
				return false;
			}

			graphModelForCodeMap = new GraphSemanticModelForCodeMap(graphModelWithEvents);
			return true;
		}

		protected virtual bool TryToInferDacOrDacExtensionSemanticModel(ITypeSymbol dacSymbol, PXContext context, int? declarationOrder,
																		out ISemanticModel? dacSemanticModel, 
																		CancellationToken cancellationToken = default)
		{
			var regularDacSemanticModel = DacSemanticModel.InferModel(context, dacSymbol, declarationOrder, cancellationToken);

			if (regularDacSemanticModel == null)
			{
				dacSemanticModel = null;
				return false;
			}

			dacSemanticModel = new DacSemanticModelForCodeMap(regularDacSemanticModel);
			return true;
		}
	}
}
