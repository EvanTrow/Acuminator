using System;

namespace Acuminator.Vsix.Settings;

/// <summary>
/// Interface for Acuminator Code Map settings events.
/// </summary>
public interface ICodeMapSettingsEvents
{
	event EventHandler<SettingChangedEventArgs> CodeMapSettingChanged;
}
