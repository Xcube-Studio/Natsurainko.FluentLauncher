using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Components;

public partial class Configuration : ObservableObject
{
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
            EnableFullScreen = false,
            EnableIndependencyCore = false,
            GameServerAddress = string.Empty,
            GameWindowHeight = 854,
            GameWindowWidth = 480,
            GameWindowTitle = string.Empty,
            Accounts = new() { OfflineAuthenticator.Default },
            EnableDemoUser = false,
            CurrentDownloadSource= "Mcbbs",
            EnableFragmentDownload = true,
            MaxDownloadThreads = 128
        };

        configuration.CurrentAccount = configuration.Accounts[0];

        return configuration;
    }

    public static ApplicationDataContainer Container => ApplicationData.Current.LocalSettings;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        Container.Values[e.PropertyName] = JsonConvert.SerializeObject(GetType().GetProperty(e.PropertyName).GetValue(this));
    }
}

public partial class Configuration
{
    [ObservableProperty]
    private string currentGameCore;

    [ObservableProperty]
    private List<string> gameFolders;

    [ObservableProperty]
    private string currentGameFolder;

    [ObservableProperty]
    private List<string> javaRuntimes;

    [ObservableProperty]
    private string currentJavaRuntime;

    [ObservableProperty]
    private int javaVirtualMachineMemory;

    [ObservableProperty]
    private bool enableAutoMemory;

    [ObservableProperty]
    private string gameWindowTitle;

    [ObservableProperty]
    private int gameWindowWidth;

    [ObservableProperty]
    private int gameWindowHeight;

    [ObservableProperty]
    private string gameServerAddress;

    [ObservableProperty]
    private bool enableFullScreen;

    [ObservableProperty]
    private bool enableIndependencyCore;
}

public partial class Configuration
{
    [ObservableProperty]
    private List<IAccount> accounts;

    [ObservableProperty]
    private IAccount currentAccount;

    [ObservableProperty]
    private bool enableDemoUser;
}

public partial class Configuration
{
    [ObservableProperty]
    private string currentDownloadSource;

    [ObservableProperty]
    private int? maxDownloadThreads;

    [ObservableProperty]
    private bool? enableFragmentDownload;
}

public partial class Configuration
{
    [ObservableProperty]
    private double appWindowWidth = 800;

    [ObservableProperty]
    private double appWindowHeight = 600;
}