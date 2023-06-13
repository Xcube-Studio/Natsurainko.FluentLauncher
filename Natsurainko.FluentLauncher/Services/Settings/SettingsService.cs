using AppSettingsManagement;
using AppSettingsManagement.Converters;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Settings;

public partial class SettingsService : SettingsContainer
{
    //[SettingsCollection(typeof(string), "GameFolders")]
    public ObservableCollection<string> GameFolders = new();

    //[SettingsCollection(typeof(string), "JavaRuntimes")]
    public ObservableCollection<string> JavaRuntimes = new();

    //[SettingsCollection(typeof(IAccount), "Accounts")]
    //public ObservableCollection<IAccount> Accounts = new(); // TODO: Remove this

    //[SettingItem(typeof(IAccount), "CurrentAccount", Converter = typeof(AccountToJsonConverter))] // TODO: Remove this
    [SettingItem(typeof(Guid), "ActiveAccountUuid")]

    [SettingItem(typeof(string), "CurrentGameCore", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(string), "CurrentGameFolder", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(string), "CurrentJavaRuntime", Default = "", Converter = typeof(JsonStringConverter<string>))]
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
    [SettingItem(typeof(string), "CoresSortBy", Default = "Name", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(string), "CoresFilter", Default = "All", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(uint), "SettingsVersion", Default = 0u)]
    public SettingsService(ISettingsStorage storage) : base(storage)
    {
        var appsettings = ApplicationData.Current.LocalSettings;

        // Init GameFolders
        string[] gameFolders = JsonSerializer.Deserialize<string[]>(appsettings.Values["GameFolders"] as string ?? "null");
        Array.ForEach(gameFolders ?? Array.Empty<string>(), GameFolders.Add);
        GameFolders.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["GameFolders"] = JsonSerializer.Serialize(GameFolders.ToArray());
        };

        // Init JavaRuntimes
        string[] javaRuntimes = JsonSerializer.Deserialize<string[]>(appsettings.Values["JavaRuntimes"] as string ?? "null");
        Array.ForEach(javaRuntimes ?? Array.Empty<string>(), JavaRuntimes.Add);
        JavaRuntimes.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["JavaRuntimes"] = JsonSerializer.Serialize(JavaRuntimes.ToArray());
        };

        // Init Accounts
        //string accountsJson = appsettings.Values["Accounts"] as string ?? "null";
        //var jsonNode = JsonNode.Parse(accountsJson);
        //if (jsonNode is not null)
        //    foreach (var item in jsonNode.AsArray())
        //    {
        //        var accountType = (AccountType)item["Type"].GetValue<int>();
        //        IAccount account = accountType switch
        //        {
        //            AccountType.Offline => item.Deserialize<OfflineAccount>(),
        //            AccountType.Microsoft => item.Deserialize<MicrosoftAccount>(),
        //            AccountType.Yggdrasil => item.Deserialize<YggdrasilAccount>(),
        //            _ => null
        //        };
        //        Accounts.Add(account);
        //    }
        //Accounts.CollectionChanged += (sender, e) =>
        //{
        //    var jsonArray = new JsonArray();
        //    foreach (var item in Accounts)
        //    {
        //        // Use derived types to store all properties
        //        if (item is OfflineAccount offlineAccount)
        //            jsonArray.Add(offlineAccount);
        //        else if (item is MicrosoftAccount microsoftAccount)
        //            jsonArray.Add(microsoftAccount);
        //        else if ((item is YggdrasilAccount yggdrasilAccount))
        //            jsonArray.Add(yggdrasilAccount);
        //    }

        //    appsettings.Values["Accounts"] = jsonArray.ToJsonString();
        //};

        // Migrate settings data structures from old versions
        Migrate();
    }

    private void Migrate()
    {
        //ApplicationData.Current.LocalSettings.Values["SettingsVersion"] = 0u; // TODO: testing only, to be removed
        if (SettingsVersion == 0u) // Version 0: Before Release 2.1.8.0
        {
            MigrateFrom_2_1_8_0();
            SettingsVersion = 1;
        }
        //if (SettingsVersion == 1) // Version 1: Release vNext
        //{
        //    SettingsVersion = 2;
        //}
    }

    private static void MigrateFrom_2_1_8_0()
    {
        if (!MsixPackageUtils.IsPackaged)
            return;

        var appsettings = ApplicationData.Current.LocalSettings;

        // Migrate the list of accounts from ApplicationData.Current.LocalSettings to LocalFolder/settings/accounts.json
        string accountsJson = appsettings.Values["Accounts"] as string ?? "null";
        JsonNode jsonNode = JsonNode.Parse(accountsJson) ?? new JsonArray();

        string localDataPath = LocalStorageService.LocalFolderPath;
        string accountSettingsDir = Path.Combine(localDataPath, "settings");
        if (!Directory.Exists(accountSettingsDir))
            Directory.CreateDirectory(accountSettingsDir);

        string accountSettingsPath = Path.Combine(accountSettingsDir, "accounts.json");
        File.WriteAllText(accountSettingsPath, jsonNode.ToString());

        //appsettings.Values.Remove("Accounts"); // TODO: Uncomment this after testing

        // Migrate to storing the GUID of the active account in ApplicationData.Current.LocalSettings

        // Read the old settings entry CurrentAccount in ApplicationData.Current.LocalSettings
        if (appsettings.Values["CurrentAccount"] is not string oldCurrentAccountJson)
            return;
        if (JsonNode.Parse(oldCurrentAccountJson) is not JsonNode currentAccountJsonNode)
            return;

        // Set new setting ActiveAccountUuid and remove the old one
        if (Guid.TryParse(currentAccountJsonNode["Uuid"].GetValue<string>(), out Guid currentAccountUuid))
        {
            appsettings.Values["ActiveAccountUuid"] = currentAccountUuid;
        }
        //appsettings.Values.Remove("CurrentAccount"); // TODO: Uncomment this after testing
    }
}
