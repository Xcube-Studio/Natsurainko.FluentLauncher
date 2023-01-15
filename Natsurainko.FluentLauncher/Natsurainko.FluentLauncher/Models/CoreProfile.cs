using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore = Natsurainko.FluentLauncher.Components.FluentCore.GameCore;

namespace Natsurainko.FluentLauncher.Models;

public partial class CoreProfile : ObservableObject
{
    public CoreProfile(string file, GameCore core, DateTime? dateTime, bool enable, LaunchSetting setting)
    {
        FilePath = file;
        launchSetting = setting;
        lastLaunchTime = dateTime;
        enableSpecialSetting = enable;

        minecraftFolder = core.Root.FullName;
        id = core.Id;
    }

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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore}));
    }
}
