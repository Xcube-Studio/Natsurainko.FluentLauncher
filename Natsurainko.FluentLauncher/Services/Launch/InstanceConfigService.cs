using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Launch;

class InstanceConfigService
{
    private readonly Dictionary<(string mcFolderPath, string instanceId), GameConfig> _instanceConfig = new();

    public GameConfig GetConfig(MinecraftInstance instance)
    {
        var key = (instance.MinecraftFolderPath, instance.InstanceId);

        // Return the instance config if it is already loaded
        if (_instanceConfig.ContainsKey(key))
            return _instanceConfig[key];

        // Load and cache instance config
        string type = instance.Version.Type switch
        {
            MinecraftVersionType.Release => "release",
            MinecraftVersionType.OldBeta => "old_beta",
            MinecraftVersionType.OldAlpha => "old_alpha",
            MinecraftVersionType.PreRelease or MinecraftVersionType.Snapshot => "snapshot",
            _ => ""
        };
        var configGuid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"{instance.MinecraftFolderPath}:{instance.InstanceId}:{type}")));
        var configsFolder = Path.Combine(LocalStorageService.LocalFolderPath, "GameConfigsFolder");

        if (!Directory.Exists(configsFolder))
            Directory.CreateDirectory(configsFolder);

        var configFile = Path.Combine(configsFolder, $"{configGuid}.json");

        if (!File.Exists(configFile))
            return new GameConfig { FilePath = configFile };

        GameConfig instanceConfig;

        try
        {
            instanceConfig = JsonNode.Parse(File.ReadAllText(configFile)).Deserialize<GameConfig>()!;
        }
        catch
        {
            instanceConfig = new GameConfig();
        }

        instanceConfig.FilePath = configFile;
        _instanceConfig[key] = instanceConfig;

        return instanceConfig;
    }
}
