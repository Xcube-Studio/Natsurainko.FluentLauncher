using AppSettingsManagement;
using AppSettingsManagement.Converters;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Settings;

partial class SettingsService : SettingsContainer
{
    //[SettingsCollection(typeof(string), "GameFolders")]
    public ObservableCollection<string> GameFolders = new();

    //[SettingsCollection(typeof(string), "JavaRuntimes")]
    public ObservableCollection<string> JavaRuntimes = new();

    //[SettingsCollection(typeof(IAccount), "Accounts")]
    public ObservableCollection<IAccount> Accounts = new();

    [SettingItem(typeof(string), "CurrentGameCore", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(string), "CurrentGameFolder", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(string), "CurrentJavaRuntime", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(IAccount), "CurrentAccount", Converter = typeof(AccountToJsonConverter))]
    [SettingItem(typeof(int), "JavaVirtualMachineMemory", Default = 1024, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(bool), "EnableAutoMemory", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(bool), "EnableAutoJava", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(bool), "EnableFullScreen", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(bool), "EnableIndependencyCore", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(string), "GameServerAddress", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(int), "GameWindowHeight", Default = 480, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(int), "GameWindowWidth", Default = 854, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(string), "GameWindowTitle", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(bool), "EnableDemoUser", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(bool), "AutoRefresh", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(bool), "UseDeviceFlowAuth", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(string), "CurrentDownloadSource", Default = "Mcbbs", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(bool), "EnableFragmentDownload", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(int), "MaxDownloadThreads", Default = 128, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(string), "CurrentLanguage", Default = "en-US, English", Converter = typeof(JsonStringConverter<string>))] // TODO: remove default value; set to system language if null
    [SettingItem(typeof(double), "AppWindowHeight", Default = 500, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(double), "AppWindowWidth", Default = 950, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(bool), "FinishGuide", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public SettingsService(ISettingsStorage storage) : base(storage)
    {
        var appsettings = ApplicationData.Current.LocalSettings;

        // Init GameFolders
        string[] gameFolders = JsonSerializer.Deserialize<string[]>(appsettings.Values["GameFolders"] as string);
        Array.ForEach(gameFolders, GameFolders.Add);
        GameFolders.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["GameFolders"] = JsonSerializer.Serialize(GameFolders.ToArray());
        };

        // Init JavaRuntimes
        string[] javaRuntimes = JsonSerializer.Deserialize<string[]>(appsettings.Values["JavaRuntimes"] as string);
        Array.ForEach(javaRuntimes, JavaRuntimes.Add);
        JavaRuntimes.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["JavaRuntimes"] = JsonSerializer.Serialize(JavaRuntimes.ToArray());
        };

        // Init Accounts
        string accountsJson = appsettings.Values["Accounts"] as string;
        var jsonNode = JsonNode.Parse(accountsJson);
        foreach (var item in jsonNode.AsArray())
        {
            var accountType = (AccountType)item["Type"].GetValue<int>();
            IAccount account = accountType switch
            {
                AccountType.Offline => item.Deserialize<OfflineAccount>(),
                AccountType.Microsoft => item.Deserialize<MicrosoftAccount>(),
                AccountType.Yggdrasil => item.Deserialize<YggdrasilAccount>(),
                _ => null
            };
            Accounts.Add(account);
        }
        Accounts.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["Accounts"] = JsonSerializer.Serialize(Accounts.ToArray());
        };

    }
}
