using FluentLauncher.Infra.Settings;
using FluentLauncher.Infra.Settings.Converters;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Management;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Services.Settings;

public partial class SettingsService : SettingsContainer
{
    #region New IFluentCoreSettingsService Property

    public ObservableCollection<string> MinecraftFolders { get; private set; } = new();
    public ObservableCollection<string> Javas { get; private set; } = new();

    [SettingItem(Default = "", Converter = typeof(JsonStringConverter<string>))]
    public partial string ActiveMinecraftFolder { get; set; }

    //[SettingItem(typeof(GameInfo), "ActiveGameInfo", Converter = typeof(JsonStringConverter<GameInfo>))]
    [SettingItem]
    public partial string? ActiveInstanceId { get; set; }

    [SettingItem(Default = "", Converter = typeof(JsonStringConverter<string>))]
    public partial string ActiveJava { get; set; }

    [SettingItem(Default = 1024, Converter = typeof(JsonStringConverter<int>))]
    public partial int JavaMemory { get; set; }

    #endregion

    [SettingItem(Default = true, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableAutoMemory { get; set; }

    [SettingItem(Default = true, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableAutoJava { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableFullScreen { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableIndependencyCore { get; set; }

    [SettingItem(Default = "", Converter = typeof(JsonStringConverter<string>))]
    public partial string GameServerAddress { get; set; }

    [SettingItem(Default = 480, Converter = typeof(JsonStringConverter<int>))]
    public partial int GameWindowHeight { get; set; }

    [SettingItem(Default = 854, Converter = typeof(JsonStringConverter<int>))]
    public partial int GameWindowWidth { get; set; }

    [SettingItem(Default = "", Converter = typeof(JsonStringConverter<string>))]
    public partial string GameWindowTitle { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableDemoUser { get; set; }

    [SettingItem(Default = true, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool AutoRefresh { get; set; }

    [SettingItem]
    public partial Guid? ActiveAccountUuid { get; set; }

    [SettingItem(Default = "Mojang", Converter = typeof(JsonStringConverter<string>))]
    public partial string CurrentDownloadSource { get; set; }

    [SettingItem(Default = true, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool EnableFragmentDownload { get; set; }

    [SettingItem(Default = 128, Converter = typeof(JsonStringConverter<int>))]
    public partial int MaxDownloadThreads { get; set; }

    [SettingItem(Default = "en-US, English", Converter = typeof(JsonStringConverter<string>))] // TODO: remove default value; set to system language if null
    public partial string CurrentLanguage { get; set; }

    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))] //TODO: remove this
    public partial int NavigationViewDisplayMode { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool NavigationViewIsPaneOpen { get; set; }

    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))]
    public partial int DisplayTheme { get; set; }

    [SettingItem(Default = 1, Converter = typeof(JsonStringConverter<int>))]
    public partial int BackgroundMode { get; set; }

    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))]
    public partial int MicaKind { get; set; }

    [SettingItem(Default = "", Converter = typeof(JsonStringConverter<string>))]
    public partial string ImageFilePath { get; set; }

    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))]
    public partial int SolidSelectedIndex { get; set; }

    [SettingItem(Converter = typeof(JsonStringConverter<Windows.UI.Color>))]
    public partial Windows.UI.Color? CustomBackgroundColor { get; set; }

    [SettingItem(Converter = typeof(JsonStringConverter<Windows.UI.Color>))]
    public partial Windows.UI.Color? CustomThemeColor { get; set; }

    [SettingItem(Default = true, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool UseSystemAccentColor { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool UseNarrowMargin { get; set; }


    [SettingItem(Default = 500, Converter = typeof(JsonStringConverter<double>))]
    public partial double AppWindowHeight { get; set; }

    [SettingItem(Default = 950, Converter = typeof(JsonStringConverter<double>))]
    public partial double AppWindowWidth { get; set; }

    [SettingItem(Default = WinUIEx.WindowState.Normal, Converter = typeof(JsonStringConverter<WinUIEx.WindowState>))]
    public partial WinUIEx.WindowState AppWindowState { get; set; }

    [SettingItem(Default = false, Converter = typeof(JsonStringConverter<bool>))]
    public partial bool FinishGuide { get; set; }


    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))]
    public partial int CoresSortByIndex { get; set; }

    [SettingItem(Default = 0, Converter = typeof(JsonStringConverter<int>))]
    public partial int CoresFilterIndex { get; set; }

    [SettingItem(Default = 0u)]
    public partial uint SettingsVersion { get; set; }

    public SettingsService(ISettingsStorage storage) : base(storage)
    {
        // Configure JsonSerializerContext for NativeAOT-compatible JsonStringConverter
        JsonStringConverterConfig.SerializerContext = FLSerializerContext.Default;

        var appsettings = ApplicationData.Current.LocalSettings;

        // Migrate settings data structures from old versions
        Migrate();

        // Init MinecraftFolders
        string? minecraftFoldersJson = appsettings.Values["MinecraftFolders"] as string;

        string[] minecraftFolders;
        if (minecraftFoldersJson is not null)
            minecraftFolders = JsonSerializer.Deserialize(minecraftFoldersJson, FLSerializerContext.Default.StringArray) ?? [];
        else
            minecraftFolders = [];

        Array.ForEach(minecraftFolders, MinecraftFolders.Add);
        MinecraftFolders.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["MinecraftFolders"] = JsonSerializer.Serialize(MinecraftFolders.ToArray(), FLSerializerContext.Default.StringArray);
        };

        // Init Javas
        string? javaRuntimesJson = appsettings.Values["Javas"] as string;

        string[] javaRuntimes;
        if (javaRuntimesJson is not null)
            javaRuntimes = JsonSerializer.Deserialize(javaRuntimesJson, FLSerializerContext.Default.StringArray) ?? [];
        else
            javaRuntimes = [];

        Array.ForEach(javaRuntimes, Javas.Add);
        Javas.CollectionChanged += (sender, e) =>
        {
            appsettings.Values["Javas"] = JsonSerializer.Serialize(Javas.ToArray(), FLSerializerContext.Default.StringArray);
        };
    }

    private void Migrate()
    {
        if (CurrentDownloadSource == "Mcbbs")
            CurrentDownloadSource = "Bmclapi";

        if (SettingsVersion == 0u) // Version 0: Before Release 2.1.8.0
        {
            MigrateFrom_2_1_8_0();
            SettingsVersion = 1u;
        }

        if (SettingsVersion == 1u) // Version 1: Before Release 2.1.13.0
        {
            MigrateFrom_2_1_13_0();
            SettingsVersion = 2u;
        }

        if (SettingsVersion == 2u) // Version 2: Before Release 2.3.0.0
        {
            MigrateFrom_2_3_0_0();
            SettingsVersion = 3u;
        }

        //if (SettingsVersion == N) // Version N: Release vNext
        //{
        //    SettingsVersion = N + 1;
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

        appsettings.Values.Remove("Accounts");

        // Migrate to storing the GUID of the active account in ApplicationData.Current.LocalSettings

        // Read the old settings entry CurrentAccount in ApplicationData.Current.LocalSettings
        if (appsettings.Values["CurrentAccount"] is not string oldCurrentAccountJson)
            return;
        if (JsonNode.Parse(oldCurrentAccountJson) is not JsonNode currentAccountJsonNode)
            return;

        // Set new setting ActiveAccountUuid and remove the old one
        if (Guid.TryParse(currentAccountJsonNode["Uuid"]?.GetValue<string>(), out Guid currentAccountUuid))
        {
            appsettings.Values["ActiveAccountUuid"] = currentAccountUuid;
        }
        appsettings.Values.Remove("CurrentAccount");
    }

    private static void MigrateFrom_2_3_0_0()
    {
        if (!MsixPackageUtils.IsPackaged)
            return;

        var appsettings = ApplicationData.Current.LocalSettings;

        // Store the client id of the active MinecraftInstance/GameInfo instead of its serialized JSON
        string accountsJson = appsettings.Values["ActiveGameInfo"] as string ?? "null";
        JsonNode? clientIdNode = JsonNode.Parse(accountsJson)?["AbsoluteId"];
        if (clientIdNode is null) return;

        string? clientId = null;
        try
        {
            clientId = clientIdNode.GetValue<string>();
        }
        catch (Exception) { }

        if (clientId is not null)
            appsettings.Values["ActiveInstanceId"] = clientId;
    }
}
