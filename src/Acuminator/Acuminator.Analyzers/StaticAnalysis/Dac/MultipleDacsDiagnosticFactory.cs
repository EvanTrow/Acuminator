using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;

/// <summary>
/// A factory class for diagnostic about multiple DACs.
/// </summary>
internal class MultipleDacsDiagnosticFactory : MultipleRootSymbolsDiagnosticFactoryBase
{
	public static MultipleDacsDiagnosticFactory Instance { get; } = new MultipleDacsDiagnosticFactory();

	protected override DiagnosticDescriptor GetDescriptorForTwoRootSymbols() => 
		Descriptors.PX1117_DacExtensionExtendsTwoDacs;

	protected override DiagnosticDescriptor GetDescriptorFor_3_To_5_RootSymbols() => 
		Descriptors.PX1117_DacExtensionExtends_3_To_5_Dacs;

	protected override DiagnosticDescriptor GetDescriptorForMoreThanFiveRootSymbols() => 
		Descriptors.PX1117_DacExtensionExtendsMoreThanFiveDacs;
}
