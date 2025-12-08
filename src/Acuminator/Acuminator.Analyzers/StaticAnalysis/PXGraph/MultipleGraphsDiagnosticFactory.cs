using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;

/// <summary>
/// A factory class for diagnostic about multiple graphs.
/// </summary>
internal class MultipleGraphsDiagnosticFactory : MultipleRootSymbolsDiagnosticFactoryBase
{
	public static MultipleGraphsDiagnosticFactory Instance { get; } = new MultipleGraphsDiagnosticFactory();

	protected override DiagnosticDescriptor GetDescriptorForTwoRootSymbols() => 
		Descriptors.PX1117_GraphExtensionExtendsTwoGraphs;

	protected override DiagnosticDescriptor GetDescriptorFor_3_To_5_RootSymbols() => 
		Descriptors.PX1117_GraphExtensionExtends_3_To_5_Graphs;

	protected override DiagnosticDescriptor GetDescriptorForMoreThanFiveRootSymbols() => 
		Descriptors.PX1117_GraphExtensionExtendsMoreThanFiveGraphs;
}
