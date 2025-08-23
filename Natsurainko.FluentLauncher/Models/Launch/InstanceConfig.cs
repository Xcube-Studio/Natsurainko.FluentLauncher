using CommunityToolkit.Mvvm.ComponentModel;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Natsurainko.FluentLauncher.Models.Launch;

internal partial class InstanceConfig(string filePath) : ObservableObject
{
    [JsonIgnore]
    public string FilePath { get; set; } = filePath;

    [ObservableProperty]
    public partial string NickName { get; set; }

    [ObservableProperty]
    public partial bool EnableSpecialSetting { get; set; }

    [ObservableProperty]
    public partial bool EnableIndependencyCore { get; set; }

    [ObservableProperty]
    public partial bool EnableFullScreen { get; set; }

    [ObservableProperty]
    public partial int GameWindowWidth { get; set; } = 854;

    [ObservableProperty]
    public partial int GameWindowHeight { get; set; } = 480;

    [ObservableProperty]
    public partial string ServerAddress { get; set; }

    [ObservableProperty]
    public partial string GameWindowTitle { get; set; }

    [ObservableProperty]
    public partial Account? Account { get; set; }

    [ObservableProperty]
    public partial bool EnableTargetedAccount { get; set; }

    [ObservableProperty]
    public partial IEnumerable<string>? VmParameters { get; set; }

    [ObservableProperty]
    public partial DateTime? LastLaunchTime { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!string.IsNullOrEmpty(FilePath))
        {
            File.WriteAllText(
                FilePath,
                JsonSerializer.Serialize(this, InstanceConfigSerializerContext.Default.InstanceConfig));
        }
    }
}
