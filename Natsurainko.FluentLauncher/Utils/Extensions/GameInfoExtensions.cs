using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using System;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class MinecraftInstanceExtensions
{
    public static bool IsSupportMod(this MinecraftInstance MinecraftInstance)
    {
        if (MinecraftInstance.IsVanilla) return false;

        var loaders = MinecraftInstance.GetModLoaders().Select(x => x.Type).ToArray();

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

    public static string GetModsDirectory(this MinecraftInstance MinecraftInstance) => Path.Combine(GetGameDirectory(MinecraftInstance), "mods");

    public static string GetSavesDirectory(this MinecraftInstance MinecraftInstance) => Path.Combine(GetGameDirectory(MinecraftInstance), "saves");

    public static void UpdateLastLaunchTimeToNow(this MinecraftInstance MinecraftInstance)
    {
        var config = MinecraftInstance.GetConfig();

        // Update launch time
        var launchTime = DateTime.Now;
        config.LastLaunchTime = launchTime;
    }
}
