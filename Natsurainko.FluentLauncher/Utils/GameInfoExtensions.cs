using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

        var coreProfile = JsonSerializer.Deserialize<GameSpecialConfig>(File.ReadAllText(configFile));
        coreProfile.FilePath = configFile;

        return coreProfile;
    }
}
