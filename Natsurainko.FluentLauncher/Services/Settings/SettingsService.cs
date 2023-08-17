using AppSettingsManagement;
using AppSettingsManagement.Converters;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Settings;

public partial class SettingsService : SettingsContainer, IFluentCoreSettingsService
{
    #region New IFluentCoreSettingsService Property

    public ObservableCollection<string> MinecraftFolders { get; private set; } = new();
    public ObservableCollection<string> Javas { get; private set; } = new();

    [SettingItem(typeof(string), "ActiveMinecraftFolder", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(GameInfo), "ActiveGameInfo", Converter = typeof(JsonStringConverter<GameInfo>))]
    [SettingItem(typeof(string), "ActiveJava", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(int), "JavaMemory", Default = 1024, Converter = typeof(JsonStringConverter<int>))]

    #endregion

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
    [SettingItem(typeof(Guid), "ActiveAccountUuid")]

    [SettingItem(typeof(string), "CurrentDownloadSource", Default = "Mcbbs", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(bool), "EnableFragmentDownload", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(int), "MaxDownloadThreads", Default = 128, Converter = typeof(JsonStringConverter<int>))]

    [SettingItem(typeof(string), "CurrentLanguage", Default = "en-US, English", Converter = typeof(JsonStringConverter<string>))] // TODO: remove default value; set to system language if null
    [SettingItem(typeof(int), "NavigationViewDisplayMode", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(int), "DisplayTheme", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(int), "BackgroundMode", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(bool), "UseNewHomePage", Default = false, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(double), "TintLuminosityOpacity", Default = 0.64, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(double), "TintOpacity", Default = 0, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(bool), "EnableDefaultAcrylicBrush", Default = true, Converter = typeof(JsonStringConverter<bool>))]
    [SettingItem(typeof(string), "ImageFilePath", Default = "", Converter = typeof(JsonStringConverter<string>))]
    [SettingItem(typeof(int), "SolidSelectedIndex", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(Windows.UI.Color), "SolidCustomColor", Converter = typeof(JsonStringConverter<Windows.UI.Color>))]

    [SettingItem(typeof(double), "AppWindowHeight", Default = 500, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(double), "AppWindowWidth", Default = 950, Converter = typeof(JsonStringConverter<double>))]
    [SettingItem(typeof(bool), "FinishGuide", Default = false, Converter = typeof(JsonStringConverter<bool>))]

    [SettingItem(typeof(int), "CoresSortByIndex", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(int), "CoresFilterIndex", Default = 0, Converter = typeof(JsonStringConverter<int>))]
    [SettingItem(typeof(int), "CoresLayoutIndex", Default = 0, Converter = typeof(JsonStringConverter<int>))]

    [SettingItem(typeof(uint), "SettingsVersion", Default = 0u)]
    public SettingsService(ISettingsStorage storage) : base(storage)
    {
        var appsettings = ApplicationData.Current.LocalSettings;

        // Migrate settings data structures from old versions
        Migrate();

        // Init MinecraftFolders
        string[] minecraftFolders = JsonSerializer.Deserialize<string[]>(appsettings.Values["MinecraftFolders"] as string ?? "null");
        Array.ForEach(minecraftFolders ?? Array.Empty<string>(), MinecraftFolders.Add);
        MinecraftFolders.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["MinecraftFolders"] = JsonSerializer.Serialize(MinecraftFolders.ToArray());
        };

        // Init Javas
        string[] javaRuntimes = JsonSerializer.Deserialize<string[]>(appsettings.Values["Javas"] as string ?? "null");
        Array.ForEach(javaRuntimes ?? Array.Empty<string>(), Javas.Add);
        Javas.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["Javas"] = JsonSerializer.Serialize(Javas.ToArray());
        };
    }

    private void Migrate()
    {
        // ApplicationData.Current.LocalSettings.Values["SettingsVersion"] = 1u; // TODO: testing only, to be removed
        if (SettingsVersion == 0u) // Version 0: Before Release 2.1.8.0
        {
            MigrateFrom_2_1_8_0();
            SettingsVersion = 1;
        }

        if (SettingsVersion == 1) // Version 0: Before Release 2.1.13.0
        {
            MigrateFrom_2_1_13_0();
            SettingsVersion = 2;
        }

        //if (SettingsVersion == 1) // Version 1: Release vNext
        //{
        //    SettingsVersion = 2;
        //}
    }

    private static void MigrateFrom_2_1_13_0()
    {
        if (!MsixPackageUtils.IsPackaged)
            return;

        var appsettings = ApplicationData.Current.LocalSettings;

        if (appsettings.Values["GameFolders"] is string oldGameFolders)
            appsettings.Values["MinecraftFolders"] = oldGameFolders;

        if (appsettings.Values["JavaRuntimes"] is string oldJavaRuntimes)
            appsettings.Values["Javas"] = oldJavaRuntimes;

        if (appsettings.Values["CurrentGameFolder"] is string oldCurrentGameFolder)
            appsettings.Values["ActiveMinecraftFolder"] = oldCurrentGameFolder;

        if (appsettings.Values["CurrentJavaRuntime"] is string oldCurrentJavaRuntime)
            appsettings.Values["ActiveJava"] = oldCurrentJavaRuntime;

        if (appsettings.Values["JavaVirtualMachineMemory"] is string oldJavaVirtualMachineMemory)
            appsettings.Values["JavaMemory"] = oldJavaVirtualMachineMemory;
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
