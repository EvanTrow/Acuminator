#nullable enable

using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class SystemNonAuditDacFieldNodeViewModel(DacMemberCategoryNodeViewModel dacMemberCategoryVM, TreeNodeViewModel parent, 
													 DacFieldInfo fieldInfo, bool isExpanded) : 
							DacFieldNodeViewModelBase(dacMemberCategoryVM, parent, fieldInfo, isExpanded)
	{
		public override Icon NodeIcon => Icon.DacSystemNonAuditField;

		public override bool IconDependsOnCurrentTheme => true;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
