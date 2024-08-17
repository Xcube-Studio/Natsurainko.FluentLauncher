using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Management.ModLoaders;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class MinecraftInstanceExtensions
{
    public static GameConfig GetConfig(this MinecraftInstance MinecraftInstance)
    {
        var configGuid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"{MinecraftInstance.MinecraftFolderPath}:{MinecraftInstance.AbsoluteId}:{MinecraftInstance.Type}")));
        var configsFolder = Path.Combine(LocalStorageService.LocalFolderPath, "GameConfigsFolder");

        if (!Directory.Exists(configsFolder)) 
            Directory.CreateDirectory(configsFolder);

        var configFile = Path.Combine(configsFolder, $"{configGuid}.json");

        if (!File.Exists(configFile)) 
            return new GameConfig { FilePath = configFile };

        GameConfig coreProfile;

        try 
        { 
            coreProfile = JsonNode.Parse(File.ReadAllText(configFile)).Deserialize<GameConfig>()!; 
        }
        catch 
        {
            coreProfile = new GameConfig(); 
        }

        coreProfile.FilePath = configFile;

        return coreProfile;
    }

    public static bool IsSupportMod(this MinecraftInstance MinecraftInstance)
    {
        if (MinecraftInstance.IsVanilla) return false;

        var loaders = MinecraftInstance.GetModLoaders().Select(x => x.LoaderType).ToArray();

        if (!(loaders.Contains(ModLoaderType.Forge) ||
            loaders.Contains(ModLoaderType.Fabric) ||
            loaders.Contains(ModLoaderType.NeoForge) ||
            loaders.Contains(ModLoaderType.Quilt) ||
            loaders.Contains(ModLoaderType.LiteLoader)))
            return false;

        return true;
    }

    public static string GetGameDirectory(this MinecraftInstance MinecraftInstance)
    {
        var config = MinecraftInstance.GetConfig();

        if (config.EnableSpecialSetting)
        {
            if (config.EnableIndependencyCore)
                return Path.Combine(MinecraftInstance.MinecraftFolderPath, "versions", MinecraftInstance.AbsoluteId);
            else return MinecraftInstance.MinecraftFolderPath;
        }

        if (App.GetService<SettingsService>().EnableIndependencyCore)
            return Path.Combine(MinecraftInstance.MinecraftFolderPath, "versions", MinecraftInstance.AbsoluteId);

        return MinecraftInstance.MinecraftFolderPath;
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
