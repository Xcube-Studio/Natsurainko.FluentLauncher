using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Components.Parse;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.DefaultComponents.Parse;

/// <summary>
/// 依赖库解析器的默认实现
/// </summary>
public class DefaultLibraryParser : BaseLibraryParser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameInfo">要解析的游戏核心</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultLibraryParser(GameInfo gameInfo) : base(gameInfo) { }

    public override void EnumerateLibraries(
        out IReadOnlyList<LibraryElement> enabledLibraries,
        out IReadOnlyList<LibraryElement> enabledNativesLibraries)
    {
        var libraries = new List<LibraryElement>();
        var natives = new List<LibraryElement>();

        var libsNode = JsonNode.Parse(File.ReadAllText(_gameInfo.VersionJsonPath))["libraries"].AsArray();

        foreach (var libNode in libsNode)
        {
            var jsonRules = libNode["rules"];
            var jsonNatives = libNode["natives"];

            if (jsonRules != null)
                if (!GetLibraryEnable(jsonRules.Deserialize<IEnumerable<RuleModel>>()))
                    continue;

            var libJsonNode = libNode.Deserialize<LibraryJsonNode>();

            if (jsonNatives != null) libJsonNode.Name += ":" + libJsonNode.Natives[EnvironmentUtils.PlatformName].Replace("${arch}", EnvironmentUtils.SystemArch);

            var relativePath = StringExtensions.FormatLibraryNameToRelativePath(libJsonNode.Name);
            var absolutePath = Path.Combine(_gameInfo.MinecraftFolderPath, "libraries", relativePath);

            var libraryElement = new LibraryElement
            {
                RelativePath = relativePath,
                AbsolutePath = absolutePath,
                IsNativeLibrary = jsonNatives != null
            };

            GetLibraryChecksumAndUrl(libraryElement, libNode, libJsonNode);

            if (libraryElement.IsNativeLibrary)
                natives.Add(libraryElement);
            else libraries.Add(libraryElement);
        }

        if (_gameInfo.IsInheritedFrom)
        {
            var parser = new DefaultLibraryParser(_gameInfo.InheritsFrom);
            parser.EnumerateLibraries(out var enabledLibraries1, out var enabledNativesLibraries1);

            ((List<LibraryElement>)enabledLibraries1).AddRange(libraries);
            ((List<LibraryElement>)enabledNativesLibraries1).AddRange(natives);

            enabledLibraries = enabledLibraries1;
            enabledNativesLibraries = enabledNativesLibraries1;
            return;
        }

        enabledLibraries = libraries;
        enabledNativesLibraries = natives;
    }

    /// <summary>
    /// 判断一个依赖库在当前平台是否被启用
    /// </summary>
    /// <param name="rules"></param>
    /// <returns></returns>
    private static bool GetLibraryEnable(IEnumerable<RuleModel> rules)
    {
        bool windows, linux, osx;
        windows = linux = osx = false;

        foreach (var item in rules)
        {
            if (item.Action == "allow")
            {
                if (item.System == null)
                {
                    windows = linux = osx = true;
                    continue;
                }

                foreach (var os in item.System)
                    switch (os.Value)
                    {
                        case "windows":
                            windows = true;
                            break;
                        case "linux":
                            linux = true;
                            break;
                        case "osx":
                            osx = true;
                            break;
                    }
            }
            else if (item.Action == "disallow")
            {
                if (item.System == null)
                {
                    windows = linux = osx = false;
                    continue;
                }

                foreach (var os in item.System)
                    switch (os.Value)
                    {
                        case "windows":
                            windows = false;
                            break;
                        case "linux":
                            linux = false;
                            break;
                        case "osx":
                            osx = false;
                            break;
                    }
            }
        }

        return EnvironmentUtils.PlatformName switch
        {
            "windows" => windows,
            "linux" => linux,
            "osx" => osx,
            _ => false,
        };
    }

    /// <summary>
    /// 解析依赖库的校验码与下载Url（如果可能）
    /// </summary>
    /// <param name="libraryElement"></param>
    /// <param name="jsonNode"></param>
    /// <param name="libraryJsonNode"></param>
    private static void GetLibraryChecksumAndUrl(LibraryElement libraryElement, JsonNode jsonNode, LibraryJsonNode libraryJsonNode)
    {
        if (libraryElement.IsNativeLibrary)
        {
            if (libraryJsonNode.Natives != null)
            {
                var nativeName = libraryJsonNode.Natives[EnvironmentUtils.PlatformName].Replace("${arch}", EnvironmentUtils.SystemArch);
                libraryElement.Checksum = libraryJsonNode.Downloads.Classifiers[nativeName].Sha1;
                libraryElement.Url = libraryJsonNode.Downloads.Classifiers[nativeName].Url;
            }

            if (jsonNode["name"].GetValue<string>().Contains("natives"))
            {
                libraryElement.Checksum = libraryJsonNode.Downloads.Artifact.Sha1;
                libraryElement.Url = libraryJsonNode.Downloads.Artifact.Url;
            }
            return;
        }

        if (libraryJsonNode.Downloads?.Artifact != null)
        {
            libraryElement.Checksum = libraryJsonNode.Downloads.Artifact.Sha1;
            libraryElement.Url = libraryJsonNode.Downloads.Artifact.Url;

            return;
        }

        if (libraryElement.Url == null && jsonNode["url"] != null)
            libraryElement.Url = (jsonNode["url"].GetValue<string>() + libraryElement.RelativePath).Replace('\\', '/');

        if (libraryElement.Checksum == null && jsonNode["checksums"] != null)
            libraryElement.Checksum = jsonNode["checksums"].AsArray()[0].GetValue<string>();
    }
}
