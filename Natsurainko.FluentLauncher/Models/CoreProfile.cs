using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Model.Launch;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace Natsurainko.FluentLauncher.Models;

public partial class CoreProfile : ObservableObject
{
    [JsonIgnore]
    public string FilePath { get; set; }

    [JsonIgnore]
    [ObservableProperty]
    private DateTime? lastLaunchTime;

    [JsonIgnore]
    [ObservableProperty]
    private string minecraftFolder;

    [JsonIgnore]
    [ObservableProperty]
    private string id;

    [JsonIgnore]
    [ObservableProperty]
    private bool enableSpecialSetting;

    [JsonIgnore]
    [ObservableProperty]
    private LaunchSetting launchSetting;

    [JsonIgnore]
    [ObservableProperty]
    private JvmSetting jvmSetting;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!string.IsNullOrEmpty(FilePath))
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }
}

public class JvmSetting
{
    public string JvmParameters { get; set; }
}