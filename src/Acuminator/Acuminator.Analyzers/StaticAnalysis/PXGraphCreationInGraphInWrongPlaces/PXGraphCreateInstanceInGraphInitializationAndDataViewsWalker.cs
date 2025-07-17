using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces
{
	/// <summary>
	/// Recursive walker that reports graph creation in graph initialization and data views.
	/// </summary>
	public class PXGraphCreateInstanceInGraphInitializationAndDataViewsWalker : PXGraphCreateInstanceWalkerBase
	{
		protected override DiagnosticDescriptor Descriptor { get; }

		public PXGraphCreateInstanceInGraphInitializationAndDataViewsWalker(SymbolAnalysisContext context, PXContext pxContext, 
																			DiagnosticDescriptor descriptor) : 
																		base(context, pxContext)
		{
			Descriptor = descriptor.CheckIfNull();
		}
	}
}
