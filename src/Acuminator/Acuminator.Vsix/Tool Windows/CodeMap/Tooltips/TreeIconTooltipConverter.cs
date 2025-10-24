#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="TreeNodeViewModel"/> node to the icon tooltip.
	/// </summary>
	[ValueConversion(sourceType: typeof(TreeNodeViewModel), targetType: typeof(string))]
	public class TreeIconTooltipConverter : IValueConverter
	{
		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Icon icon = GetIcon(value);
			var node = GetNode(value);

			return icon switch
			{
				Icon.DacKeyField 								=> VSIXResource.CodeMap_ExtraInfo_DacKeyIconTooltip,
				Icon.DacAuditField 								=> VSIXResource.CodeMap_ExtraInfo_DacAuditFieldIconTooltip,
				Icon.DacAuditFieldsCategory 					=> VSIXResource.CodeMap_ExtraInfo_DacAuditFieldsCategoryIconTooltip,
				Icon.DacSystemNonAuditField						=> VSIXResource.CodeMap_ExtraInfo_DacSystemNonAuditFieldIconTooltip,
				Icon.DacSystemNonAuditFieldsCategory			=> VSIXResource.CodeMap_ExtraInfo_DacSystemNonAuditFieldsCategoryIconTooltip,
				Icon.Settings 									=> VSIXResource.CodeMap_ExtraInfo_PXSetupViewIconTooltip,
				Icon.Filter 									=> VSIXResource.CodeMap_ExtraInfo_PXFilterViewIconTooltip,
				Icon.Processing when node is ViewNodeViewModel	=> VSIXResource.CodeMap_ExtraInfo_ProcessingViewIconTooltip,
				Icon.Processing 
				when node is GraphNodeViewModel graphVM			=> graphVM.IsGraph 
																	? VSIXResource.CodeMap_ExtraInfo_ProcessingGraphIconTooltip
																	: VSIXResource.CodeMap_ExtraInfo_ProcessingGraphExtensionIconTooltip,
				Icon.ProjectionDac 								=> VSIXResource.CodeMap_ExtraInfo_ProjectionDacIndicatorTooltip,
				Icon.ProjectionAttribute 						=> VSIXResource.CodeMap_Icon_ProjectionAttributeTooltip,
				Icon.PXAccumulatorDac 							=> VSIXResource.CodeMap_ExtraInfo_PXAccumulatorDacIndicatorTooltip,
				Icon.PXAccumulatorAttribute 					=> VSIXResource.CodeMap_Icon_PXAccumulatorAttributeTooltip,
				_ 												=> null
			};
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

		private Icon GetIcon(object viewModel) =>
			viewModel switch
			{
				TreeNodeViewModel treeNode 	=> treeNode.NodeIcon,
				IconViewModel iconViewModel => iconViewModel.IconType,
				_ 							=> Icon.None,
			};

		private TreeNodeViewModel? GetNode(object viewModel) =>
			viewModel is IconViewModel iconViewModel
				? iconViewModel.Node
				: viewModel as TreeNodeViewModel;
	}
}
