﻿using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Components.Parse;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Nrk.FluentCore.DefaultComponets.Parse;

/// <summary>
/// 依赖材质解析器的默认实现
/// </summary>
public class DefaultAssetParser : BaseAssetParser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameInfo">要解析的游戏核心</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultAssetParser(GameInfo gameInfo) : base(gameInfo) { }

    public override AssetElement GetAssetIndexJson()
    {
        var assetIndex = JsonNode.Parse(File.ReadAllText(_gameInfo.IsInheritedFrom ? _gameInfo.InheritsFrom.VersionJsonPath : _gameInfo.VersionJsonPath))
            ["assetIndex"].Deserialize<AssstIndexJsonNode>();

        return new AssetElement
        {
            Name = assetIndex.Id + ".json",
            Checksum = assetIndex.Sha1,
            Url = assetIndex.Url,
            AbsolutePath = _gameInfo.AssetsIndexJsonPath,
            RelativePath = _gameInfo.AssetsIndexJsonPath.Replace(Path.Combine(_gameInfo.MinecraftFolderPath, "assets"), string.Empty).TrimStart('\\')
        };
    }

    public override IEnumerable<AssetElement> EnumerateAssets()
    {
        if (string.IsNullOrEmpty(_gameInfo.AssetsIndexJsonPath)) yield break; //未找到 assets\indexes\assetindex.json

        var assets = JsonNode.Parse(File.ReadAllText(_gameInfo.AssetsIndexJsonPath))["objects"].Deserialize<Dictionary<string, AssetJsonNode>>();

        foreach ( var keyValuePair in assets )
        {
            var hashPath = Path.Combine(keyValuePair.Value.Hash[..2], keyValuePair.Value.Hash);
            var relativePath = Path.Combine("objects", hashPath);
            var absolutePath = Path.Combine(_gameInfo.MinecraftFolderPath, "assets", relativePath);

            yield return new AssetElement
            {
                Name = keyValuePair.Key,
                Checksum = keyValuePair.Value.Hash,
                RelativePath = relativePath,
                AbsolutePath = absolutePath,
                Url = "https://resources.download.minecraft.net/" + hashPath.Replace('\\', '/')
            };
        }
    }
}
