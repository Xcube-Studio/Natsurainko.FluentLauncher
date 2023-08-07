using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Components.Launch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.DefaultComponents.Launch;

/// <summary>
/// 游戏定位器的默认实现
/// </summary>
public class DefaultGameLocator : BaseGameLocator
{
    /// <param name="folder">.minecraft 目录绝对路径</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultGameLocator(string folder) : base(folder) { }

    /// <param name="directory">.minecraft 目录 <see cref="DirectoryInfo"/> 对象</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultGameLocator(DirectoryInfo directory) : base(directory.FullName) { }

    public override IEnumerable<GameInfo> EnumerateGames()
    {
        var versionsDirectory = new DirectoryInfo(Path.Combine(MinecraftFolderPath, "versions"));

        if (!versionsDirectory.Exists) yield break; // 不存在 .versions 文件夹

        var enumedGames = new List<GameInfo>();
        var inheritedFromGames = new Dictionary<VersionJsonEntity, GameInfo>();

        foreach (var dir in versionsDirectory.EnumerateDirectories())
        {
            var jsonFile = new FileInfo(Path.Combine(dir.FullName, dir.Name + ".json"));
            VersionJsonEntity jsonEntity = default;

            if (!jsonFile.Exists) continue; // 不存在 version.json

            try { jsonEntity = JsonSerializer.Deserialize<VersionJsonEntity>(File.ReadAllText(jsonFile.FullName)); } catch { }

            if (jsonEntity == null) continue; // version.json 读取失败

            var gameInfo = new GameInfo
            {
                AbsoluteId = jsonEntity.Id,
                Name = jsonEntity.Id,
                Type = jsonEntity.Type,
                IsInheritedFrom = false,
                MinecraftFolderPath = MinecraftFolderPath,
                VersionJsonPath = jsonFile.FullName
            };

            if (jsonEntity.AssetIndex != null)
                gameInfo.AssetsIndexJsonPath = Path.Combine(MinecraftFolderPath, "assets", "indexes", $"{jsonEntity.AssetIndex?.Id}.json");

            if (!string.IsNullOrEmpty(jsonEntity.InheritsFrom))
            {
                gameInfo.IsInheritedFrom = true;
                inheritedFromGames.Add(jsonEntity, gameInfo);
                continue;
            }

            TryGetIsVanillaAndAbsoluteVersion(gameInfo, jsonEntity);

            if (!gameInfo.IsInheritedFrom) gameInfo.JarPath = jsonFile.FullName.Replace(".json", ".jar");
            if (!gameInfo.IsInheritedFrom && gameInfo.IsVanilla) enumedGames.Add(gameInfo);

            yield return gameInfo;
        }

        foreach (var keyValuePair in inheritedFromGames)
        {
            var inheritsFrom = enumedGames.FirstOrDefault(info => info.AbsoluteId.Equals(keyValuePair.Key.InheritsFrom), null);
            if (inheritsFrom == null) continue; // 继承的核心未找到

            keyValuePair.Value.InheritsFrom = inheritsFrom;
            keyValuePair.Value.JarPath ??= inheritsFrom.JarPath;
            TryGetIsVanillaAndAbsoluteVersion(keyValuePair.Value, keyValuePair.Key);

            yield return keyValuePair.Value;
        }
    }

    public override IReadOnlyList<GameInfo> GetGames(out IReadOnlyList<string> errorGameNames)
    {
        var games = new List<GameInfo>();
        var errorGames = new List<string>();

        var versionsDirectory = new DirectoryInfo(Path.Combine(MinecraftFolderPath, "versions"));

        var inheritedFromGames = new Dictionary<VersionJsonEntity, GameInfo>();

        if (!versionsDirectory.Exists) // 不存在 .versions 文件夹
        {
            errorGameNames = errorGames;
            return games;
        }

        foreach (var dir in versionsDirectory.EnumerateDirectories())
        {
            var jsonFile = new FileInfo(Path.Combine(dir.FullName, dir.Name + ".json"));
            VersionJsonEntity jsonEntity = default;

            if (!jsonFile.Exists) continue; // 不存在 version.json

            try { jsonEntity = JsonSerializer.Deserialize<VersionJsonEntity>(File.ReadAllText(jsonFile.FullName)); } catch { errorGames.Add(dir.Name); }

            if (jsonEntity == null) continue; // version.json 读取失败

            var gameInfo = new GameInfo
            {
                AbsoluteId = jsonEntity.Id,
                Name = jsonEntity.Id,
                Type = jsonEntity.Type,
                IsInheritedFrom = false,
                MinecraftFolderPath = MinecraftFolderPath,
                VersionJsonPath = jsonFile.FullName
            };

            var assetsIndexFile = Path.Combine(MinecraftFolderPath, "assets", "indexes", $"{jsonEntity.AssetIndex?.Id}.json");
            var jarFile = jsonFile.FullName.Replace(".json", ".jar");

            if (File.Exists(jarFile)) gameInfo.JarPath = jarFile;
            if (File.Exists(assetsIndexFile)) gameInfo.AssetsIndexJsonPath = assetsIndexFile;

            if (!string.IsNullOrEmpty(jsonEntity.InheritsFrom))
            {
                gameInfo.IsInheritedFrom = true;
                inheritedFromGames.Add(jsonEntity, gameInfo);
                continue;
            }

            TryGetIsVanillaAndAbsoluteVersion(gameInfo, jsonEntity);

            games.Add(gameInfo);
        }

        errorGameNames = errorGames;

        foreach (var keyValuePair in inheritedFromGames)
        {
            var inheritsFrom = games.FirstOrDefault(info => info.AbsoluteId.Equals(keyValuePair.Key.InheritsFrom), null);
            if (inheritsFrom == null)
            {
                errorGames.Add(keyValuePair.Key.Id);
                continue;
            } // 继承的核心未找到

            keyValuePair.Value.InheritsFrom = inheritsFrom;
            keyValuePair.Value.JarPath ??= inheritsFrom.JarPath;
            TryGetIsVanillaAndAbsoluteVersion(keyValuePair.Value, keyValuePair.Key);

            games.Add(keyValuePair.Value);
        }

        return games;
    }

    public override GameInfo GetGame(string absoluteId)
    {
        var jsonFile = new FileInfo(Path.Combine(MinecraftFolderPath, "versions", absoluteId, absoluteId + ".json"));

        if (!jsonFile.Exists) // 不存在对应的 version.json 文件
            return null;

        VersionJsonEntity jsonEntity = default;

        try { jsonEntity = JsonSerializer.Deserialize<VersionJsonEntity>(File.ReadAllText(jsonFile.FullName)); } catch { }

        if (jsonEntity == null) return null; // version.json 读取失败

        var gameInfo = new GameInfo
        {
            AbsoluteId = jsonEntity.Id,
            Name = jsonEntity.Id,
            Type = jsonEntity.Type,
            IsInheritedFrom = false,
            MinecraftFolderPath = MinecraftFolderPath,
            VersionJsonPath = jsonFile.FullName
        };

        var assetsIndexFile = Path.Combine(MinecraftFolderPath, "assets", "indexes", $"{jsonEntity.AssetIndex?.Id}.json");
        var jarFile = jsonFile.FullName.Replace(".json", ".jar");

        if (File.Exists(jarFile)) gameInfo.JarPath = jarFile;
        if (File.Exists(assetsIndexFile)) gameInfo.AssetsIndexJsonPath = assetsIndexFile;

        if (!string.IsNullOrEmpty(jsonEntity.InheritsFrom))
        {
            var inheritsFrom = GetGame(jsonEntity.InheritsFrom);
            if (inheritsFrom == null) // 继承的核心未找到
                return null;

            gameInfo.IsInheritedFrom = true;
            gameInfo.InheritsFrom = inheritsFrom;
            gameInfo.JarPath ??= inheritsFrom.JarPath;
        }

        TryGetIsVanillaAndAbsoluteVersion(gameInfo, jsonEntity);

        return gameInfo;
    }

    private static void TryGetIsVanillaAndAbsoluteVersion(GameInfo gameInfo, VersionJsonEntity jsonEntity)
    {
        gameInfo.IsVanilla = true;

        var ensureMainClass = jsonEntity.MainClass switch
        {
            "net.minecraft.client.main.Main"
            or "net.minecraft.launchwrapper.Launch"
            or "com.mojang.rubydung.RubyDung" => true,
            _ => false,
        };

        if (!string.IsNullOrEmpty(jsonEntity.InheritsFrom)
            || !ensureMainClass
            || (ensureMainClass
                && (jsonEntity.MinecraftArguments?.Contains("--tweakClass")).GetValueOrDefault()
                && !(jsonEntity.MinecraftArguments?.Contains("--tweakClass net.minecraft.launchwrapper.AlphaVanillaTweaker")).GetValueOrDefault())
            || (ensureMainClass
                && (jsonEntity.Arguments?.Game?.Where(e => e.ValueKind.Equals(JsonValueKind.String)
                && e.GetString().Equals("--tweakClass")).Any()).GetValueOrDefault()))
            gameInfo.IsVanilla = false;

        if (gameInfo.IsVanilla) gameInfo.AbsoluteVersion = gameInfo.AbsoluteId;
        else if (gameInfo.IsInheritedFrom) gameInfo.AbsoluteVersion = gameInfo.InheritsFrom.AbsoluteId;
        else
        {
            var json = JsonNode.Parse(File.ReadAllText(gameInfo.VersionJsonPath));

            var pathchs = json["patches"]; // hmcl合并核心版本号读取
            var clientVersion = json["clientVersion"]; // pcl合并核心版本号读取

            if (pathchs != null) gameInfo.AbsoluteVersion = pathchs[0]["version"].GetValue<string>();
            if (clientVersion != null) gameInfo.AbsoluteVersion = clientVersion.GetValue<string>();
        }
    }
}
