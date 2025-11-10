using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings;

public class SettingChangedEventArgs(string settingName) : EventArgs
{
	public string SettingName { get; } = settingName.CheckIfNullOrWhiteSpace();
}
