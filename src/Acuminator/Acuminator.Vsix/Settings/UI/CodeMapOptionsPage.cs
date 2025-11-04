#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Microsoft.VisualStudio.Shell;

using Constants = Acuminator.Vsix.Utilities.Constants;

namespace Acuminator.Vsix
{
	[ComVisible(true)]
	public class CodeMapOptionsPage : DialogPage, ICodeMapSettingsEvents
	{
		public const string PageTitle = "Code Map";
		private const string NodeExpansionCategoryName = "Node Expansion";

		private bool _settingsChanged;

		public event EventHandler<SettingChangedEventArgs>? CodeMapSettingChanged;

		protected override IWin32Window Window
		{
			get 
			{
				var baseWindow = base.Window;

				if (baseWindow is not PropertyGrid propertyGrid)
					return baseWindow;

				propertyGrid.PropertySort = PropertySort.Categorized;
				return propertyGrid;
			}
		}

		private bool _expandRegularNodes = CodeMapSettings.DefaultExpandRegularNodes;

		[DefaultValue(CodeMapSettings.DefaultExpandRegularNodes)]
		[CategoryFromResources(nameof(VSIXResource.Category_NodeExpansion), NodeExpansionCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeMap_ExpandRegularNodes_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeMap_ExpandRegularNodes_Description))]
		public bool ExpandRegularNodes
		{
			get => _expandRegularNodes;
			set
			{
				if (_expandRegularNodes != value)
				{
					_expandRegularNodes = value;
					_settingsChanged = true;
				}
			}
		}

		private bool _expandAttributeNodes = CodeMapSettings.DefaultExpandAttributeNodes;

		[DefaultValue(CodeMapSettings.DefaultExpandAttributeNodes)]
		[CategoryFromResources(nameof(VSIXResource.Category_NodeExpansion), NodeExpansionCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeMap_ExpandAttributeNodes_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeMap_ExpandAttributeNodes_Description))]
		public bool ExpandAttributeNodes
		{
			get => _expandAttributeNodes;
			set
			{
				if (_expandAttributeNodes != value)
				{
					_expandAttributeNodes = value;
					_settingsChanged = true;
				}
			}
		}

		public override void ResetSettings()
		{
			_expandRegularNodes   = CodeMapSettings.DefaultExpandRegularNodes;
			_expandAttributeNodes = CodeMapSettings.DefaultExpandAttributeNodes;
			
			base.ResetSettings();

			_settingsChanged = false;
			OnCodeMapSettingChanged(Constants.Settings.All);
		}

		public override void SaveSettingsToStorage()
		{
			base.SaveSettingsToStorage();

			if (_settingsChanged)
			{
				_settingsChanged = false;
				OnCodeMapSettingChanged(Constants.Settings.All);
			}
		}

		private void OnCodeMapSettingChanged(string setting) => 
			CodeMapSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
	}
}
