using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Natsurainko.FluentLauncher.Classes.Data.Launch;

internal partial class GameSpecialConfig : ObservableObject
{
    [JsonIgnore]
    public string FilePath { get; set; }

    [ObservableProperty]
    private bool enableSpecialSetting;

    [ObservableProperty]
    private DateTime? lastLaunchTime;

    [ObservableProperty]
    private string minecraftFolder;

    [ObservableProperty]
    private bool enableIndependencyCore;

    [ObservableProperty]
    private string serverAddress;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private bool isFullscreen;

    [ObservableProperty]
    private string windowTitle;

    [ObservableProperty]
    private IEnumerable<string> vmParameters;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!string.IsNullOrEmpty(FilePath))
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                IncludeFields = false,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));
    }
}
