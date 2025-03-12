using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacGroupingNodeForFieldEventHandlerBaseViewModel<TFieldEventHandlerInfo> : DacGroupingNodeBaseViewModel
	where TFieldEventHandlerInfo : GraphFieldEventHandlerInfoBase<TFieldEventHandlerInfo>
	{
		public ImmutableArray<TFieldEventHandlerInfo> AllFieldEvents { get; }

		public DacGroupingNodeForFieldEventHandlerBaseViewModel(GraphEventCategoryNodeViewModel graphEventsCategoryVM, string dacName,
																IEnumerable<TFieldEventHandlerInfo> fieldEvents, bool isExpanded) :
															base(graphEventsCategoryVM, dacName, isExpanded)
		{
			AllFieldEvents = fieldEvents?.ToImmutableArray() ?? ImmutableArray.Create<TFieldEventHandlerInfo>();
		}
	}
}