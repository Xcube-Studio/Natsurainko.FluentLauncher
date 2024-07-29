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

internal static class GameInfoExtensions
{
    public static GameConfig GetConfig(this GameInfo gameInfo)
    {
        var configGuid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"{gameInfo.MinecraftFolderPath}:{gameInfo.AbsoluteId}:{gameInfo.Type}")));
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

    public static bool IsSupportMod(this GameInfo gameInfo)
    {
        if (gameInfo.IsVanilla) return false;

        var loaders = gameInfo.GetModLoaders().Select(x => x.LoaderType).ToArray();

        if (!(loaders.Contains(ModLoaderType.Forge) ||
            loaders.Contains(ModLoaderType.Fabric) ||
            loaders.Contains(ModLoaderType.NeoForge) ||
            loaders.Contains(ModLoaderType.Quilt) ||
            loaders.Contains(ModLoaderType.LiteLoader)))
            return false;

        return true;
    }

    public static string GetGameDirectory(this GameInfo gameInfo)
    {
        var config = gameInfo.GetConfig();

        if (config.EnableSpecialSetting)
        {
            if (config.EnableIndependencyCore)
                return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);
            else return gameInfo.MinecraftFolderPath;
        }

        if (App.GetService<SettingsService>().EnableIndependencyCore)
            return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);

        return gameInfo.MinecraftFolderPath;
    }

    public static string GetModsDirectory(this GameInfo gameInfo) => Path.Combine(GetGameDirectory(gameInfo), "mods");

    public static string GetSavesDirectory(this GameInfo gameInfo) => Path.Combine(GetGameDirectory(gameInfo), "saves");

    public static void UpdateLastLaunchTimeToNow(this GameInfo gameInfo)
    {
        var config = gameInfo.GetConfig();

        // Update launch time
        var launchTime = DateTime.Now;
        config.LastLaunchTime = launchTime;
    }
}
