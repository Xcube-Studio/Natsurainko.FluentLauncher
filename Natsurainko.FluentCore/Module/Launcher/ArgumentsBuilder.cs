using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.Text;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Natsurainko.FluentCore.Module.Launcher;

public class ArgumentsBuilder : IArgumentsBuilder
{
    public GameCore GameCore { get; private set; }

    public LaunchSetting LaunchSetting { get; private set; }

    public ArgumentsBuilder(GameCore gameCore, LaunchSetting launchSetting)
    {
        this.GameCore = gameCore;
        this.LaunchSetting = launchSetting;
    }

    public IEnumerable<string> Build()
    {
        foreach (var item in GetFrontArguments())
            yield return item;

        yield return this.GameCore.MainClass;

        foreach (var item in GetBehindArguments())
            yield return item;
    }

    public IEnumerable<string> GetFrontArguments()
    {
        var keyValuePairs = new Dictionary<string, string>()
        {
            { "${launcher_name}", "Natsurainko.FluentCore" },
            { "${launcher_version}", "3" },
            { "${classpath_separator}", Path.PathSeparator.ToString() },
            { "${classpath}", this.GetClasspath().ToPath() },
            { "${client}", this.GameCore.ClientFile.FileInfo.FullName.ToPath() },
            { "${min_memory}", this.LaunchSetting.JvmSetting.MinMemory.ToString() },
            { "${max_memory}", this.LaunchSetting.JvmSetting.MaxMemory.ToString() },
            { "${library_directory}", Path.Combine(this.GameCore.Root.FullName, "libraries").ToPath() },
            {
                "${version_name}",
                string.IsNullOrEmpty(this.GameCore.InheritsFrom)
                ? this.GameCore.Id
                : this.GameCore.InheritsFrom
            },
            {
                "${natives_directory}",
                this.LaunchSetting.NativesFolder != null && this.LaunchSetting.NativesFolder.Exists
                ? this.LaunchSetting.NativesFolder.FullName.ToString()
                : Path.Combine(this.GameCore.Root.FullName, "versions", this.GameCore.Id, "natives").ToPath()
            }
        };

        if (!Directory.Exists(keyValuePairs["${natives_directory}"]))
            Directory.CreateDirectory(keyValuePairs["${natives_directory}"].Trim('\"'));

        var args = new string[]
        {
            "-Xms${min_memory}M",
            "-Xmx${max_memory}M",
            "-Dminecraft.client.jar=${client}",
        }.ToList();

        foreach (var item in GetEnvironmentJVMArguments())
            args.Add(item);

        if (this.LaunchSetting.JvmSetting.GCArguments == null)
            DefaultSettings.DefaultGCArguments.ToList().ForEach(item => args.Add(item));
        else this.LaunchSetting.JvmSetting.GCArguments.ToList().ForEach(item => args.Add(item));

        if (this.LaunchSetting.JvmSetting.AdvancedArguments == null)
            DefaultSettings.DefaultAdvancedArguments.ToList().ForEach(item => args.Add(item));
        else this.LaunchSetting.JvmSetting.AdvancedArguments.ToList().ForEach(item => args.Add(item));

        if (this.LaunchSetting.XmlOutputSetting.Enable && this.LaunchSetting.XmlOutputSetting.LogConfigFile != null)
        {
            args.Add("-Dlog4j.configurationFile=${path}");
            keyValuePairs.Add("${path}", this.LaunchSetting.XmlOutputSetting.LogConfigFile.FullName.ToPath());
        }

        args.Add("-Dlog4j2.formatMsgNoLookups=true");

        foreach (var item in this.GameCore.FrontArguments)
            args.Add(item);

        foreach (var item in args)
            yield return item.Replace(keyValuePairs);
    }

    public IEnumerable<string> GetBehindArguments()
    {
        var keyValuePairs = new Dictionary<string, string>()
        {
            { "${auth_player_name}" , this.LaunchSetting.Account.Name },
            { "${version_name}" , this.GameCore.Id },
            { "${assets_root}" , Path.Combine(this.GameCore.Root.FullName, "assets").ToPath() },
            { "${assets_index_name}" , Path.GetFileNameWithoutExtension(this.GameCore.AssetIndexFile.FileInfo.FullName) },
            { "${auth_uuid}" , this.LaunchSetting.Account.Uuid.ToString("N") },
            { "${auth_access_token}" , this.LaunchSetting.Account.AccessToken },
            { "${user_type}" , "Mojang" },
            { "${version_type}" , this.GameCore.Type },
            { "${user_properties}" , "{}" },
            { "${game_assets}" , Path.Combine(this.GameCore.Root.FullName, "assets").ToPath() },
            { "${auth_session}" , this.LaunchSetting.Account.AccessToken },
            {
                "${game_directory}" ,
                    (this.LaunchSetting.EnableIndependencyCore && (bool)this.LaunchSetting.WorkingFolder?.Exists
                        ? this.LaunchSetting.WorkingFolder.FullName
                        : GameCore.Root.FullName).ToPath()
            },
        };

        var args = this.GameCore.BehindArguments.ToList();

        if (this.LaunchSetting.GameWindowSetting != null)
        {
            args.Add($"--width {this.LaunchSetting.GameWindowSetting.Width}");
            args.Add($"--height {this.LaunchSetting.GameWindowSetting.Height}");

            if (this.LaunchSetting.GameWindowSetting.IsFullscreen)
                args.Add("--fullscreen");
        }

        if (this.LaunchSetting.IsDemoUser)
            args = args.Append("--demo").ToList();

        if (this.LaunchSetting.ServerSetting != null && !string.IsNullOrEmpty(this.LaunchSetting.ServerSetting.IPAddress) && this.LaunchSetting.ServerSetting.Port != 0)
        {
            args.Add($"--server {this.LaunchSetting.ServerSetting.IPAddress}");
            args.Add($"--port {this.LaunchSetting.ServerSetting.Port}");
        }

        foreach (var item in args)
            yield return item.Replace(keyValuePairs);
    }

    public string GetClasspath()
    {
        var loads = new List<IResource>();

        this.GameCore.LibraryResources.ForEach(x =>
        {
            if (x.IsEnable && !x.IsNatives)
                loads.Add(x);
        });

        loads.Add(this.GameCore.ClientFile);

        return string.Join(Path.PathSeparator.ToString(), loads.Select(x => x.ToFileInfo().FullName));
    }

    public static IEnumerable<string> GetEnvironmentJVMArguments()
    {
        switch (EnvironmentInfo.GetPlatformName())
        {
            case "windows":
                yield return "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump";
                if (Environment.OSVersion.Version.Major == 10)
                {
                    yield return "-Dos.name=\"Windows 10\"";
                    yield return "-Dos.version=10.0";
                }
                break;
            case "osx":
                yield return "-XstartOnFirstThread";
                break;
        }

        if (EnvironmentInfo.Arch == "32")
            yield return "-Xss1M";
    }
}
