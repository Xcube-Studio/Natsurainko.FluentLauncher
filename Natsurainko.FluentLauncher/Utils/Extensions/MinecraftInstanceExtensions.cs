using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class MinecraftInstanceExtensions
{
    private static InstanceConfigService s_instanceConfigService = App.GetService<InstanceConfigService>();

    public static bool IsSupportMod(this MinecraftInstance instance)
    {
        if (instance.IsVanilla) return false;

        var loaders = instance.GetModLoaders().Select(x => x.Type).ToArray();

        if (!(loaders.Contains(ModLoaderType.Forge) ||
            loaders.Contains(ModLoaderType.Fabric) ||
            loaders.Contains(ModLoaderType.NeoForge) ||
            loaders.Contains(ModLoaderType.Quilt) ||
            loaders.Contains(ModLoaderType.LiteLoader)))
            return false;

        return true;
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
}
