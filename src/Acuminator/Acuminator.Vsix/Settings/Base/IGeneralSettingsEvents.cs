using System;

namespace Acuminator.Vsix.Settings;

/// <summary>
/// Interface for Acuminator General settings events.
/// </summary>
public interface IGeneralSettingsEvents
{
	event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
	event EventHandler<SettingChangedEventArgs> CodeAnalysisSettingChanged;
}
