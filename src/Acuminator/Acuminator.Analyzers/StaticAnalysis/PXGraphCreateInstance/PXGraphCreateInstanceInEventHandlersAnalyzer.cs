using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	public class PXGraphCreateInstanceInEventHandlersAnalyzer : LooseEventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers,
				Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers_NonISV);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventHandlerLooseInfo eventHandlerInfo)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext);

			methodSyntax?.Accept(walker);
		}

		private class Walker(SymbolAnalysisContext context, PXContext pxContext) : PXGraphCreateInstanceWalkerBase(context, pxContext)
		{
			private static readonly DiagnosticDescriptor _px1045_isv	= Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers;
			private static readonly DiagnosticDescriptor _px1045_nonIsv = Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers_NonISV;

			protected override DiagnosticDescriptor Descriptor =>
				Settings.IsvSpecificAnalyzersEnabled
					? _px1045_isv
					: _px1045_nonIsv;
		}
	}
}
