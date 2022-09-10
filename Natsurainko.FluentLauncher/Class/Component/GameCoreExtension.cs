using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Mapping;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Class.Component;

public static class GameCoreExtension
{
    public static async Task<CustomLaunchSetting> GetCustomLaunchSetting(this GameCore gameCore)
        => await GameCoreLocator.GetGameCoreCustomLaunchSetting(gameCore.Root.FullName, gameCore.Id);

    public static async Task<string> GetLaunchWorkingFolder(this GameCore gameCore)
    {
        var customLaunchSetting = await gameCore.GetCustomLaunchSetting();
        string WorkingFolder = string.Empty;

        if (customLaunchSetting.Enable && customLaunchSetting.EnableIndependencyCore)
            WorkingFolder = Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id);
        else if (!customLaunchSetting.Enable && ConfigurationManager.AppSettings.EnableIndependencyCore.GetValueOrDefault())
            WorkingFolder = Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id);
        else WorkingFolder = gameCore.Root.FullName;

        return WorkingFolder;
    }

    public static string GetLaunchWorkingFolder(this GameCore gameCore, CustomLaunchSetting customLaunchSetting)
    {
        string WorkingFolder = string.Empty;

        if (customLaunchSetting.Enable && customLaunchSetting.EnableIndependencyCore)
            WorkingFolder = Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id);
        else if (!customLaunchSetting.Enable && ConfigurationManager.AppSettings.EnableIndependencyCore.GetValueOrDefault())
            WorkingFolder = Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id);
        else WorkingFolder = gameCore.Root.FullName;

        return WorkingFolder;
    }

    public static async Task<string> GetStorageModFolder(this GameCore gameCore)
    {
        var customLaunchSetting = await gameCore.GetCustomLaunchSetting();
        var enableIndependencyCore = gameCore.GetEnableIndependencyCore(customLaunchSetting);

        return enableIndependencyCore
            ? Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id, "mods")
            : Path.Combine(gameCore.Root.FullName, "mods");
    }

    public static string GetStorageModFolder(this GameCore gameCore, CustomLaunchSetting customLaunchSetting)
    {
        var enableIndependencyCore = gameCore.GetEnableIndependencyCore(customLaunchSetting);

        return enableIndependencyCore
            ? Path.Combine(gameCore.Root.FullName, "versions", gameCore.Id, "mods")
            : Path.Combine(gameCore.Root.FullName, "mods");
    }

    public static async Task<bool> GetEnableIndependencyCore(this GameCore gameCore)
    {
        var customLaunchSetting = await gameCore.GetCustomLaunchSetting();

        bool @bool = false;

        if (customLaunchSetting.Enable && customLaunchSetting.EnableIndependencyCore)
            @bool = true;
        else if (!customLaunchSetting.Enable && ConfigurationManager.AppSettings.EnableIndependencyCore.GetValueOrDefault())
            @bool = true;
        else @bool = false;

        return @bool;
    }

    public static bool GetEnableIndependencyCore(this GameCore gameCore, CustomLaunchSetting customLaunchSetting)
    {
        bool @bool = false;

        if (customLaunchSetting.Enable && customLaunchSetting.EnableIndependencyCore)
            @bool = true;
        else if (!customLaunchSetting.Enable && ConfigurationManager.AppSettings.EnableIndependencyCore.GetValueOrDefault())
            @bool = true;
        else @bool = false;

        return @bool;
    }

}
