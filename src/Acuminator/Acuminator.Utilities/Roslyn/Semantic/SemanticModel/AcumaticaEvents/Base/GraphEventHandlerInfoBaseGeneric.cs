using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents
{
	/// <summary>
	/// A common generic base class for the graph event handler info.
	/// </summary>
	public abstract class GraphEventHandlerInfoBase<TEventHandlerInfo> : GraphEventHandlerInfoBase, IWriteableBaseItem<TEventHandlerInfo>
	where TEventHandlerInfo : GraphEventHandlerInfoBase<TEventHandlerInfo>
	{
		protected TEventHandlerInfo? _baseEventHandlerInfo;

		/// <summary>
		/// The base event handler info. 
		/// </summary>
		public TEventHandlerInfo? Base => _baseEventHandlerInfo;

		/// <summary>
		/// The base event handler info. 
		/// </summary>
		/// <remarks>
		/// Internal setter is used for two reasons:
		/// 1) Perfomance - to avoid allocation of objects during retrieval of overrides hierarchy.  
		/// 2) Overcomplicated architecture - the use of completely readonly objects will require a more complex <see cref="GraphEventsCollection{TEventInfoType}"/> class
		/// which will know how to create a new <typeparamref name="TEventHandlerInfo"/> event info. 
		/// This will lead to a two concrete implementations of collection for <see cref="GraphRowEventInfo"/> and <see cref="GraphFieldEventHandlerInfo"/> 
		/// or to a hard to read code in the <see cref="PXGraphEventSemanticModel.EventsCollector"/> if we choose to pass the delegates to the generic collection class. 
		/// </remarks>
		TEventHandlerInfo? IWriteableBaseItem<TEventHandlerInfo>.Base
		{
			get => Base;
			set 
			{
				_baseEventHandlerInfo = value;
				OverrideType = GetOverrideType();

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		public GraphEventHandlerOverrideType OverrideType { get; private set; }

		protected GraphEventHandlerInfoBase(MethodDeclarationSyntax? handlerNode, IMethodSymbol handlerSymbol, int declarationOrder,
											EventHandlerLooseInfo handlerLooseInfo) :
										base(handlerNode, handlerSymbol, declarationOrder, handlerLooseInfo)
		{
			OverrideType = GetOverrideType();
		}

		protected GraphEventHandlerInfoBase(MethodDeclarationSyntax? handlerNode, IMethodSymbol handlerSymbol, int declarationOrder,
											EventHandlerLooseInfo handlerLooseInfo, TEventHandlerInfo baseEventHandlerInfo) : 
										base(handlerNode, handlerSymbol, declarationOrder, handlerLooseInfo)
		{
			_baseEventHandlerInfo = baseEventHandlerInfo.CheckIfNull();
			OverrideType = GetOverrideType();

			CombineWithBaseInfo(baseEventHandlerInfo);
		}

		void IWriteableBaseItem<TEventHandlerInfo>.CombineWithBaseInfo(TEventHandlerInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		protected virtual void CombineWithBaseInfo(TEventHandlerInfo baseInfo)
		{
		}

		private GraphEventHandlerOverrideType GetOverrideType()
		{
			if (Base == null)
				return GraphEventHandlerOverrideType.None;
			else if (Symbol.IsOverride)
				return GraphEventHandlerOverrideType.CSharp;
			else
				return GraphEventHandlerOverrideType.AcumaticaEventsOverride;
		}
	}
}
