using Natsurainko.FluentCore.Extension.Windows.Extension;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Text;
using Natsurainko.Toolkits.Values;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

public class GameCore : Natsurainko.FluentCore.Model.Launch.GameCore
{
    public CoreProfile CoreProfile { get; set; }

    public FileInfo GetFileOfProfile()
    {
        var profileGuid = GuidHelper.FromString($"{Root.FullName}:{Id}:{Type}");

#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        var profilesFolder = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Natsurainko.FluentLauncher", "CoreProfiles"));
#else
        var profilesFolder = new DirectoryInfo(Path.Combine(App.StoragePath, "CoreProfiles"));
#endif

        if (!profilesFolder.Exists)
            profilesFolder.Create();

        return new FileInfo(Path.Combine(profilesFolder.FullName, $"{profileGuid}.json"));
    }

    public LaunchSetting GetLaunchSetting()
    {
        var globalSetting = new LaunchSetting
        {
            Account = App.Configuration.CurrentAccount,
            IsDemoUser = App.Configuration.EnableDemoUser,
            EnableIndependencyCore = App.Configuration.EnableIndependencyCore,
            GameWindowSetting = new()
            {
                Height = App.Configuration.GameWindowHeight,
                Width = App.Configuration.GameWindowWidth,
                IsFullscreen = App.Configuration.EnableFullScreen,
                WindowTitle = App.Configuration.GameWindowTitle
            }
        };

        if (!string.IsNullOrEmpty(App.Configuration.GameServerAddress))
            globalSetting.ServerSetting = new ServerSetting(App.Configuration.GameServerAddress);

        if (App.Configuration.JavaRuntimes.Any())
            if (App.Configuration.EnableAutoJava)
                globalSetting.JvmSetting = new(GetSuitableJava());
            else globalSetting.JvmSetting = new(App.Configuration.CurrentJavaRuntime);
        else globalSetting.JvmSetting = new();

        if (App.Configuration.EnableAutoMemory)
            globalSetting.JvmSetting.AutoSetMemory();
        else globalSetting.JvmSetting.MaxMemory = globalSetting.JvmSetting.MinMemory = App.Configuration.JavaVirtualMachineMemory;

        if (CoreProfile.EnableSpecialSetting)
        {
            globalSetting.ServerSetting = CoreProfile.LaunchSetting.ServerSetting;
            globalSetting.GameWindowSetting = CoreProfile.LaunchSetting.GameWindowSetting;
            globalSetting.EnableIndependencyCore = CoreProfile.LaunchSetting.EnableIndependencyCore;

            if ((CoreProfile.JvmSetting?.JvmParameters.Split(' ').Any()).GetValueOrDefault(false))
                globalSetting.JvmSetting.JvmArguments = CoreProfile.JvmSetting?.JvmParameters.Split(' ').ToList();
        }

        if (globalSetting.EnableIndependencyCore)
            globalSetting.WorkingFolder = new(Path.Combine(Root.FullName, "versions", Id));
        else globalSetting.WorkingFolder = new(Root.FullName);

        return globalSetting;
    }

    public string GetSuitableJava()
    {
        var regex = new Regex(@"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\?)*$");

        var javaInformations = App.Configuration.JavaRuntimes
            .Where(x => regex.IsMatch(x) && File.Exists(x))
            .ToDictionary(x => x, x => JavaHelper.GetJavaRuntimeInfo(x));

        var sameMajorJava = javaInformations.Where(kvp => kvp.Value.Version.Major.Equals(JavaVersion));

        if (!javaInformations.Any())
            return string.Empty;

        if (!sameMajorJava.Any())
            return javaInformations.MaxBy(x => x.Value.Version).Key;

        return sameMajorJava.MaxBy(x => x.Value.Version).Key;
    }

    public string MakeLaunchScript()
    {
        var launchSetting = GetLaunchSetting();
        var builder = new ArgumentsBuilder(this, launchSetting);
        var arguments = new List<string>(builder.Build());
        var stringBuilder = new StringBuilder();

        arguments.Insert(0, launchSetting.JvmSetting.Javaw.FullName.ToPath());

        stringBuilder.AppendLine("@echo off");
        stringBuilder.AppendLine($"set APPDATA={Root.Parent.FullName}");
        stringBuilder.AppendLine($"cd /{Root.FullName[0]} {Root.FullName}");
        stringBuilder.AppendLine(string.Join(' ', arguments));
        stringBuilder.AppendLine("pause");

        return stringBuilder.ToString();
    }
}
