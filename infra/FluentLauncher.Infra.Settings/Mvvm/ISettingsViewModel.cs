using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings.Mvvm;

public interface ISettingsViewModel
{
    SettingsContainer SettingsContainer { get; }
    Dictionary<string, SettingChangedEventHandler> SettingChangedEventHandlers { get; }

    /// <summary>
    /// Initialize observable properties with binding to settings container and register events for updating view model when settings are changed.
    /// </summary>
    void InitializeSettings();
    /// <summary>
    /// This should be called in the destructor. Unregister events for updating view model when settings are changed.
    /// </summary>
    void RemoveSettingsChagnedHandlers();
    /// <summary>
    /// Update the settings container with the current view model values when an observable property is changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SettingsViewModelPropertyChangedCallback(object? sender, PropertyChangedEventArgs e);
}
