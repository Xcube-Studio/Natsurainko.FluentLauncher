using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Utils;

internal static class GameInfoExtensions
{
    public static GameSpecialConfig GetSpecialConfig(this GameInfo gameInfo)
    {
        var configGuid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"{gameInfo.MinecraftFolderPath}:{gameInfo.AbsoluteId}:{gameInfo.Type}")));
        var configsFolder = Path.Combine(LocalStorageService.LocalFolderPath, "CoreSpecialConfigs");

        if (!Directory.Exists(configsFolder)) Directory.CreateDirectory(configsFolder);

        var configFile = Path.Combine(configsFolder, $"{configGuid}.json");

        if (!File.Exists(configFile)) return new GameSpecialConfig { FilePath = configFile };

        GameSpecialConfig coreProfile;
        var json = JsonNode.Parse(File.ReadAllText(configFile));

        var item = json["Account"];
        Account account = null;

        if (item != null)
        {
            var accountType = (AccountType)(item?["Type"].GetValue<int>());

            account = accountType switch
            {
                AccountType.Offline => item.Deserialize<OfflineAccount>(),
                AccountType.Microsoft => item.Deserialize<MicrosoftAccount>(),
                AccountType.Yggdrasil => item.Deserialize<YggdrasilAccount>(),
                _ => null
            };

            var obj = json.AsObject();
            obj.Remove("Account");
            coreProfile = obj.Deserialize<GameSpecialConfig>();
        }
        else coreProfile = json.Deserialize<GameSpecialConfig>();

        coreProfile.Account = account;
        coreProfile.FilePath = configFile;

        return coreProfile;
    }

    public static ExtendedGameInfo Extend(this GameInfo gameInfo)
    {
        var specialConfig = gameInfo.GetSpecialConfig();

        return new()
        {
            AbsoluteId = gameInfo.AbsoluteId,
            AbsoluteVersion = gameInfo.AbsoluteVersion,
            AssetsIndexJsonPath = gameInfo.AssetsIndexJsonPath,
            InheritsFrom = gameInfo.InheritsFrom,// == null ? null : gameInfo.Extend(),
            IsInheritedFrom = gameInfo.IsInheritedFrom,
            IsVanilla = gameInfo.IsVanilla,
            JarPath = gameInfo.JarPath,
            LastLaunchTime = specialConfig.LastLaunchTime,
            MinecraftFolderPath = gameInfo.MinecraftFolderPath,
            Name = string.IsNullOrEmpty(specialConfig.NickName) ? gameInfo.AbsoluteId : specialConfig.NickName,
            Type = gameInfo.Type,
            VersionJsonPath = gameInfo.VersionJsonPath,
        };
    }

    public static bool IsSupportMod(this GameInfo gameInfo)
    {
        if (gameInfo.IsVanilla) return false;

        var loaders = gameInfo.GetModLoaders().Select(x => x.LoaderType).ToArray();

        if (!(loaders.Contains(ModLoaderType.Forge) ||
            loaders.Contains(ModLoaderType.Fabric) ||
            loaders.Contains(ModLoaderType.Quilt) ||
            loaders.Contains(ModLoaderType.LiteLoader)))
            return false;

        return true;
    }

    public static string GetGameDirectory(this GameInfo gameInfo)
    {
        var specialConfig = gameInfo.GetSpecialConfig();

        if (specialConfig.EnableSpecialSetting)
        {
            if (specialConfig.EnableIndependencyCore)
                return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);
            else return gameInfo.MinecraftFolderPath;
        }

        if (App.GetService<SettingsService>().EnableIndependencyCore)
            return Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId);

        return gameInfo.MinecraftFolderPath;
    }
}
