#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class RowEventHandlerCategoryNodeViewModel : GraphEventHandlerCategoryNodeViewModel
	{
		public RowEventHandlerCategoryNodeViewModel(GraphNodeViewModel graphViewModel, TreeNodeViewModel parent, bool isExpanded) : 
											   base(graphViewModel, parent, GraphMemberCategory.RowEvent, isExpanded)
		{
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() =>
			GraphSemanticModel.DeclaredEventHandlers.RowInsertingEvents
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowInsertedEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowSelectingEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowSelectedEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowUpdatingEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowUpdatedEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowDeletingEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowDeletedEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowPersistingEvents)
													.Concat(GraphSemanticModel.DeclaredEventHandlers.RowPersistedEvents);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
