using CommunityToolkit.Mvvm.ComponentModel;
using Nrk.FluentCore.Classes.Datas.Authenticate;
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
    private string nickName;

    [ObservableProperty]
    private bool enableSpecialSetting;

    [ObservableProperty]
    private DateTime? lastLaunchTime;

    [ObservableProperty]
    private bool enableIndependencyCore;

    [ObservableProperty]
    private bool enableFullScreen;

    [ObservableProperty]
    private int gameWindowWidth = 854;

    [ObservableProperty]
    private int gameWindowHeight = 480;

    [ObservableProperty]
    private string serverAddress;

    [ObservableProperty]
    private string gameWindowTitle;

    [ObservableProperty]
    private Account account;

    [ObservableProperty]
    private bool enableTargetedAccount;

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
