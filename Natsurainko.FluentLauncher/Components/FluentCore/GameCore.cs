using Natsurainko.FluentCore.Extension.Windows.Extension;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.Toolkits.Text;
using Natsurainko.Toolkits.Values;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Components.FluentCore;

class GameCore : Natsurainko.FluentCore.Model.Launch.GameCore
{
    private readonly SettingsService _settings;
    private readonly AccountService _accountService;

    public GameCore(SettingsService settings, AccountService accountService)
    {
        _settings = settings;
        _accountService = accountService;
    }

    public CoreProfile CoreProfile { get; set; }

    public FileInfo GetFileOfProfile()
    {
        var profileGuid = GuidHelper.FromString($"{Root.FullName}:{Id}:{Type}");

#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        var profilesFolder = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Natsurainko.FluentLauncher", "CoreProfiles"));
#else
        var profilesFolder = new DirectoryInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, "CoreProfiles"));
#endif

        if (!profilesFolder.Exists)
            profilesFolder.Create();

        return new FileInfo(Path.Combine(profilesFolder.FullName, $"{profileGuid}.json"));
    }

    public LaunchSetting GetLaunchSetting()
    {
        var globalSetting = new LaunchSetting
        {
            Account = _accountService.ActiveAccount,
            IsDemoUser = _settings.EnableDemoUser,
            EnableIndependencyCore = _settings.EnableIndependencyCore,
            GameWindowSetting = new()
            {
                Height = _settings.GameWindowHeight,
                Width = _settings.GameWindowWidth,
                IsFullscreen = _settings.EnableFullScreen,
                WindowTitle = _settings.GameWindowTitle
            }
        };

        if (!string.IsNullOrEmpty(_settings.GameServerAddress))
            globalSetting.ServerSetting = new ServerSetting(_settings.GameServerAddress);

        if (_settings.JavaRuntimes.Any())
            if (_settings.EnableAutoJava)
                globalSetting.JvmSetting = new(GetSuitableJava());
            else globalSetting.JvmSetting = new(_settings.CurrentJavaRuntime);
        else globalSetting.JvmSetting = new();

        if (_settings.EnableAutoMemory)
            globalSetting.JvmSetting.AutoSetMemory();
        else globalSetting.JvmSetting.MaxMemory = globalSetting.JvmSetting.MinMemory = _settings.JavaVirtualMachineMemory;

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

        var javaInformations = _settings.JavaRuntimes
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
