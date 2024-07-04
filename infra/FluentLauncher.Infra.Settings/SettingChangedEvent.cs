using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings;

public class SettingChangedEventArgs
{
    public string Path { get; init; }
    public object? NewValue { get; init; }

    public SettingChangedEventArgs(string path, object? value)
    {
        Path = path;
        NewValue = value;
    }
}

public delegate void SettingChangedEventHandler(SettingsContainer sender, SettingChangedEventArgs e);
