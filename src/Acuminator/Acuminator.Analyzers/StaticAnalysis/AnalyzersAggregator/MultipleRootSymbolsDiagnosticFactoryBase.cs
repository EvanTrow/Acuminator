using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;

/// <summary>
/// A base factory class for diagnostic about multiple root symbols.
/// </summary>
internal abstract class MultipleRootSymbolsDiagnosticFactoryBase
{
	protected const int MaxDisplayedRootsCount = 5;

	protected abstract DiagnosticDescriptor GetDescriptorForTwoRootSymbols();

	protected abstract DiagnosticDescriptor GetDescriptorFor_3_To_5_RootSymbols();

	protected abstract DiagnosticDescriptor GetDescriptorForMoreThanFiveRootSymbols();

	public Diagnostic? CreateDiagnosticForMultipleRootSymbols(ITypeSymbol extensionType, List<ITypeSymbol> multipleRootsList)
	{
		if (!extensionType.IsInSourceCode())
			return null;

		var location = extensionType.Locations.FirstOrDefault();
		switch (multipleRootsList.Count)
		{
			case 2:
				string[] formatArg_2Roots =
				[
					extensionType.ToString(),
						multipleRootsList[0].ToString().SurroundWithQuotes(),
						multipleRootsList[1].ToString().SurroundWithQuotes()
				];
				var diagnostic_2Roots = Diagnostic.Create(GetDescriptorForTwoRootSymbols(), location, formatArg_2Roots);
				return diagnostic_2Roots;

			case >= 3 and <= MaxDisplayedRootsCount:
				string rootsExceptLastRoot = multipleRootsList.Take(multipleRootsList.Count - 1)
															  .Select(root => root.ToString().SurroundWithQuotes()!)
															  .Join("," + Environment.NewLine);
				string[] formatArg_From_3_To_5_Roots =
				[
					extensionType.ToString(),
						rootsExceptLastRoot,
						multipleRootsList[^1].ToString().SurroundWithQuotes()
				];

				var diagnostic_3_To_5_Roots = Diagnostic.Create(GetDescriptorFor_3_To_5_RootSymbols(), location, formatArg_From_3_To_5_Roots);
				return diagnostic_3_To_5_Roots;

			default:
				string firstFiveRoots = multipleRootsList.Take(MaxDisplayedRootsCount)
														 .Select(root => root.ToString().SurroundWithQuotes()!)
														 .Join("," + Environment.NewLine);
				int numberOfRemainingRoots = multipleRootsList.Count - MaxDisplayedRootsCount;
				string[] formatArg_MoreThan_5_Roots =
				[
					extensionType.ToString(),
					firstFiveRoots,
					numberOfRemainingRoots.ToString()
				];

				var diagnostic_MoreThan_5_Roots = Diagnostic.Create(GetDescriptorForMoreThanFiveRootSymbols(), location,
																	formatArg_MoreThan_5_Roots);
				return diagnostic_MoreThan_5_Roots;
		}
	}
}
