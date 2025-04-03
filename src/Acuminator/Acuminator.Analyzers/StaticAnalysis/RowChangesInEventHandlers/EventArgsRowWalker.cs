using System.Diagnostics.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for <code>e.Row</code> mentions
		/// </summary>
		private class EventArgsRowWalker : CSharpSyntaxWalker
		{
			private const string RowPropertyName = "Row";

			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			[MemberNotNullWhen(returnValue: true, nameof(FoundRowProperty))]
			public bool Success => FoundRowProperty != null;

			public IPropertySymbol? FoundRowProperty { get; private set; }

			public EventArgsRowWalker(SemanticModel semanticModel, PXContext pxContext)
			{
				_semanticModel = semanticModel.CheckIfNull();
				_pxContext = pxContext.CheckIfNull();
			}

			public void Reset()
			{
				FoundRowProperty = null;
			}

			public override void Visit(SyntaxNode? node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				if (node.Identifier.Text == RowPropertyName)
				{
					var propertySymbol = _semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
					var containingType = propertySymbol?.ContainingType?.OriginalDefinition;

					if (containingType != null && _pxContext.Events.EventArgTypeToEventTypeMap.ContainsKey(containingType))
					{
						FoundRowProperty = propertySymbol;
					}
				}
			}
		}

	}
}
