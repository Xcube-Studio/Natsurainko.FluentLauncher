using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.IO;
using System.Web;
using Windows.ApplicationModel;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class MinecraftInstanceExtensions
{
    private static InstanceConfigService s_instanceConfigService = App.GetService<InstanceConfigService>();

    public static bool IsSupportMod(this MinecraftInstance instance)
    {
        if (instance.IsVanilla) return false;

        if (instance is ModifiedMinecraftInstance modifiedInstance)
        {
            foreach (var item in modifiedInstance.ModLoaders)
            {
                switch (item.Type)
                {
                    case ModLoaderType.Forge:
                    case ModLoaderType.LiteLoader:
                    case ModLoaderType.Fabric:
                    case ModLoaderType.Quilt:
                    case ModLoaderType.NeoForge:
                        return true;
                }
            }
        }

        return false;
    }

    public static string GetGameDirectory(this MinecraftInstance instance)
    {
        var config = instance.GetConfig();

        if (config.EnableSpecialSetting)
        {
            if (config.EnableIndependencyCore)
                return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);
            else return instance.MinecraftFolderPath;
        }

        if (App.GetService<SettingsService>().EnableIndependencyCore)
            return Path.Combine(instance.MinecraftFolderPath, "versions", instance.InstanceId);

        return instance.MinecraftFolderPath;
    }

    public static string GetModsDirectory(this MinecraftInstance instance) => Path.Combine(GetGameDirectory(instance), "mods");

    public static string GetSavesDirectory(this MinecraftInstance instance) => Path.Combine(GetGameDirectory(instance), "saves");

    public static InstanceConfig GetConfig(this MinecraftInstance instance) => s_instanceConfigService.GetConfig(instance);

    public static string GetDisplayName(this MinecraftInstance instance)
    {
        try
        {
            var instanceConfig = instance.GetConfig();

            return string.IsNullOrEmpty(instanceConfig.NickName)
                ? instance.InstanceId
                : instanceConfig.NickName;
        }
        catch
        {
            return instance.InstanceId;
        }
    }

    public static void CreateShortcut(this MinecraftInstance instance, string? folder = null)
    {
        folder ??= Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string shortcutContent = "[{{000214A0-0000-0000-C000-000000000046}}]\r\n" +
        $"""
        Prop3=19,0
        [InternetShortcut]
        IDList=
        URL="fluent-launcher://quickLaunch/?minecraftFolder={HttpUtility.UrlEncode(instance.MinecraftFolderPath)}&instanceId={HttpUtility.UrlEncode(instance.InstanceId)}"
        IconIndex=0
        HotKey=0
        IconFile={Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\Icons", "minecraft.ico")}
        """;

        File.WriteAllText(Path.Combine(folder, $"{instance.GetDisplayName()}.url"), shortcutContent);
    }
}
