using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.Components.Launch;
using Nrk.FluentCore.DefaultComponents.Parse;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nrk.FluentCore.DefaultComponents.Launch;

/// <summary>
/// 参数构建器的默认实现
/// </summary>
public class DefaultArgumentsBuilder : BaseArgumentsBuilder<DefaultArgumentsBuilder>
{
    private bool _isCmdMode = false;
    private bool _Dlog4j2_FormatMsgNoLookups = true;

    private readonly string _librariesFolder;
    private readonly string _nativesFolder;
    private readonly string _assetsFolder;
    private string _gameDirectory;

    private IEnumerable<LibraryElement> _libraries;

    private Account _account;
    private bool _enableDemoUser;

    private string _javaPath;
    private int _minMemory;
    private int _maxMemory;

    private IEnumerable<string> _extraVmParameters;
    private IEnumerable<string> _extraGameParameters;

    public DefaultArgumentsBuilder(GameInfo gameInfo) : base(gameInfo)
    {
        _nativesFolder = Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId, "natives");
        _librariesFolder = Path.Combine(gameInfo.MinecraftFolderPath, "libraries");
        _assetsFolder = Path.Combine(gameInfo.MinecraftFolderPath, "assets");
        _gameDirectory = gameInfo.MinecraftFolderPath;
    }

    public override IEnumerable<string> Build()
    {
        var jsonNode = JsonNode.Parse(File.ReadAllText(GameInfo.VersionJsonPath));
        var entity = jsonNode.Deserialize<VersionJsonEntity>();

        var vmParameters = DefaultVmParameterParser.Parse(jsonNode);
        var gameParameters = DefaultGameParameterParser.Parse(jsonNode);

        if (GameInfo.IsInheritedFrom)
        {
            vmParameters = DefaultVmParameterParser
                .Parse(JsonNode.Parse(File.ReadAllText(GameInfo.InheritsFrom.VersionJsonPath)))
                .Union(vmParameters);
            gameParameters = DefaultGameParameterParser
                .Parse(JsonNode.Parse(File.ReadAllText(GameInfo.InheritsFrom.VersionJsonPath)))
                .Union(gameParameters);
        }

        var vmParametersReplace = new Dictionary<string, string>()
        {
            { "${launcher_name}", "Natsurainko.FluentCore" },
            { "${launcher_version}", "4" },
            { "${classpath_separator}", Path.PathSeparator.ToString() },
            { "${library_directory}", _librariesFolder.ToPathParameter() },
            { "${natives_directory}", _nativesFolder.ToPathParameter() },
            { "${classpath}", GetClasspath().ToPathParameter() },
            {
                "${version_name}", GameInfo.IsInheritedFrom
                ? GameInfo.InheritsFrom.AbsoluteId
                : GameInfo.AbsoluteId
            },
        };
        var gameParametersReplace = new Dictionary<string, string>()
        {
            { "${auth_player_name}" , _account.Name },
            { "${auth_access_token}" , _account.AccessToken },
            { "${auth_session}" , _account.AccessToken },
            { "${auth_uuid}" ,_account.Uuid.ToString("N") },
            { "${user_type}" , _account.Type.Equals(AccountType.Microsoft) ? "MSA" : "Mojang" },
            { "${user_properties}" , "{}" },
            { "${version_name}" , GameInfo.AbsoluteId },
            { "${version_type}" , GameInfo.Type },
            { "${game_assets}" , _assetsFolder.ToPathParameter() },
            { "${assets_root}" , _assetsFolder.ToPathParameter() },
            { "${game_directory}" , _gameDirectory.ToPathParameter() },
            { "${assets_index_name}" , Path.GetFileNameWithoutExtension(GameInfo.IsInheritedFrom ? GameInfo.InheritsFrom.AssetsIndexJsonPath : GameInfo.AssetsIndexJsonPath) },
        };

        if (_isCmdMode)
        {
            yield return "@echo off";
            yield return $"\r\nset APPDATA={Directory.GetParent(GameInfo.MinecraftFolderPath).FullName}";
            yield return $"\r\ncd /{GameInfo.MinecraftFolderPath[0]} {GameInfo.MinecraftFolderPath}";
            yield return $"\r\n{_javaPath.ToPathParameter()}";
        }

        yield return $"-Xms{_minMemory}M";
        yield return $"-Xmx{_maxMemory}M";
        yield return $"-Dminecraft.client.jar={GameInfo.JarPath.ToPathParameter()}";

        if (_Dlog4j2_FormatMsgNoLookups) yield return "-Dlog4j2.formatMsgNoLookups=true";
        if (_extraVmParameters != null)
            foreach (var arg in _extraVmParameters)
                yield return arg;

        foreach (var arg in DefaultVmParameterParser.GetEnvironmentJVMArguments()) yield return arg;
        foreach (var arg in vmParameters) yield return arg.ReplaceFromDictionary(vmParametersReplace);

        yield return entity.MainClass;

        foreach (var arg in gameParameters) yield return arg.ReplaceFromDictionary(gameParametersReplace);

        if (_extraGameParameters != null)
            foreach (var arg in _extraGameParameters)
                yield return arg;

        if (_enableDemoUser) yield return "--demo";

        if (_isCmdMode) yield return "\r\npause";
    }

    public override DefaultArgumentsBuilder SetJavaSettings(string javaPath, int maxMemory, int minMemory)
    {
        _javaPath = javaPath;
        _minMemory = minMemory;
        _maxMemory = maxMemory;

        return this;
    }

    public override DefaultArgumentsBuilder SetAccountSettings(Account account, bool enableDemoUser)
    {
        _account = account;
        _enableDemoUser = enableDemoUser;

        return this;
    }

    public override DefaultArgumentsBuilder SetLibraries(IEnumerable<LibraryElement> libraryElements)
    {
        _libraries = libraryElements;
        return this;
    }

    public override DefaultArgumentsBuilder AddExtraParameters(IEnumerable<string> extraVmParameters = null, IEnumerable<string> extraGameParameters = null)
    {
        _extraVmParameters = extraVmParameters;
        _extraGameParameters = extraGameParameters;

        return this;
    }

    public override DefaultArgumentsBuilder SetGameDirectory(string directory)
    {
        _gameDirectory = directory;
        return this;
    }

    public DefaultArgumentsBuilder SetCmdMode(bool isCmdMode)
    {
        _isCmdMode = isCmdMode;
        return this;
    }

    private string GetClasspath()
    {
        var classPath = string.Join(Path.PathSeparator, _libraries.Select(lib => lib.AbsolutePath));
        if (!string.IsNullOrEmpty(GameInfo.JarPath)) classPath += Path.PathSeparator + GameInfo.JarPath;

        return classPath;
    }
}
