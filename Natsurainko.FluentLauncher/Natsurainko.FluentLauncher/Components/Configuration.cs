using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Components;

public partial class Configuration : ObservableObject
{
#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
    public static string DataFilePath => Path.Combine(Directory.GetCurrentDirectory(), "ApplicationDataContainer.json");

    public static Configuration Load()
    {
        var file = new FileInfo(DataFilePath);

        if (!file.Exists)
        {
            var defaultData = Default();
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(defaultData, Formatting.Indented));
            return defaultData;
        }

        return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file.FullName));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        File.WriteAllText(DataFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
#else
    public static Configuration Load()
        => Container.Values.Any()
        ? Create()
        : Default();

    public static Configuration Create()
    {
        var configuration = new Configuration();

        foreach (var key in Container.Values.Keys)
        {
            var property = configuration.GetType().GetProperty(key);
            property.SetValue(configuration, JsonConvert.DeserializeObject((string)Container.Values[key], property.PropertyType));
        }

        return configuration;
    }

    public static ApplicationDataContainer Container => ApplicationData.Current.LocalSettings;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        Container.Values[e.PropertyName] = JsonConvert.SerializeObject(GetType().GetProperty(e.PropertyName).GetValue(this));
    }
#endif

    public static Configuration Default()
    {
        var configuration = new Configuration
        {
            CurrentGameCore = string.Empty,
            CurrentGameFolder = string.Empty,
            CurrentJavaRuntime = string.Empty,
            GameFolders = new(),
            JavaRuntimes = new(),
            JavaVirtualMachineMemory = 1024,
            EnableAutoMemory = true,
            EnableAutoJava = true,
            EnableFullScreen = false,
            EnableIndependencyCore = false,
            GameServerAddress = string.Empty,
            GameWindowHeight = 480,
            GameWindowWidth = 854,
            GameWindowTitle = string.Empty,
            Accounts = new() { OfflineAuthenticator.Default },
            EnableDemoUser = false,
            AutoRefresh = true,
            CurrentDownloadSource = "Mcbbs",
            EnableFragmentDownload = true,
            MaxDownloadThreads = 128
        };

        configuration.CurrentAccount = configuration.Accounts[0];

        return configuration;
    }

    public void ReportPropertyChanged(PropertyChangedEventArgs e)
        => OnPropertyChanged(e);
}

public partial class Configuration
{
    [ObservableProperty] [JsonIgnore]
    private string currentGameCore;

    [ObservableProperty] [JsonIgnore]
    private List<string> gameFolders;

    [ObservableProperty] [JsonIgnore]
    private string currentGameFolder;

    [ObservableProperty] [JsonIgnore]
    private List<string> javaRuntimes;

    [ObservableProperty] [JsonIgnore]
    private string currentJavaRuntime;

    [ObservableProperty] [JsonIgnore]
    private int javaVirtualMachineMemory;

    [ObservableProperty] [JsonIgnore]
    private bool enableAutoMemory;

    [ObservableProperty] [JsonIgnore]
    private bool enableAutoJava;

    [ObservableProperty] [JsonIgnore]
    private string gameWindowTitle;

    [ObservableProperty] [JsonIgnore]
    private int gameWindowWidth;

    [ObservableProperty] [JsonIgnore]
    private int gameWindowHeight;

    [ObservableProperty] [JsonIgnore]
    private string gameServerAddress;

    [ObservableProperty] [JsonIgnore]
    private bool enableFullScreen;

    [ObservableProperty] [JsonIgnore]
    private bool enableIndependencyCore;
}

public partial class Configuration
{
    [ObservableProperty] [JsonIgnore]
    private List<IAccount> accounts;

    [ObservableProperty] [JsonIgnore]
    private IAccount currentAccount;

    [ObservableProperty] [JsonIgnore]
    private bool enableDemoUser;

    [ObservableProperty] [JsonIgnore]
    private bool autoRefresh;
}

public partial class Configuration
{
    [ObservableProperty] [JsonIgnore]
    private string currentDownloadSource;

    [ObservableProperty] [JsonIgnore]
    private int? maxDownloadThreads;

    [ObservableProperty] [JsonIgnore]
    private bool? enableFragmentDownload;
}

public partial class Configuration
{
    [ObservableProperty] [JsonIgnore]
    private double appWindowWidth = 800;

    [ObservableProperty] [JsonIgnore]
    private double appWindowHeight = 600;
}

public partial class Configuration
{
    [ObservableProperty] [JsonIgnore]
    private string coresSortBy = "Name";

    [ObservableProperty] [JsonIgnore]
    private string coresFilter = "All";
}