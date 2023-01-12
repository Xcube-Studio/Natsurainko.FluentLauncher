using Natsurainko.FluentCore.Extension.Windows.Extension;
using Natsurainko.FluentCore.Extension.Windows.Service;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Values;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;

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
        var profilesFolder = new DirectoryInfo(Path.Combine(Package.Current.InstalledLocation.Path, "Natsurainko.FluentLauncher", "CoreProfiles"));
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

        if (App.Configuration.EnableAutoJava)
            globalSetting.JvmSetting = new(GetSuitableJava());
        else globalSetting.JvmSetting = new(App.Configuration.CurrentJavaRuntime);

        if (App.Configuration.EnableAutoMemory)
            globalSetting.JvmSetting.AutoSetMemory();
        else globalSetting.JvmSetting.MaxMemory = globalSetting.JvmSetting.MinMemory = App.Configuration.JavaVirtualMachineMemory;

        if (CoreProfile.EnableSpecialSetting)
        {
            globalSetting.ServerSetting = CoreProfile.LaunchSetting.ServerSetting;
            globalSetting.GameWindowSetting = CoreProfile.LaunchSetting.GameWindowSetting;
            globalSetting.EnableIndependencyCore = CoreProfile.LaunchSetting.EnableIndependencyCore;
        }

        if (globalSetting.EnableIndependencyCore)
            globalSetting.WorkingFolder = new(Path.Combine(Root.FullName, "versions", Id));

        return globalSetting;
    }

    public string GetSuitableJava()
    {
        var javaInformations = App.Configuration.JavaRuntimes.ToDictionary(x => x, x => JavaHelper.GetJavaRuntimeInfo(x));
        var sameMajorJava = javaInformations.Where(kvp => kvp.Value.Version.Major.Equals(JavaVersion));

        if (!sameMajorJava.Any())
            return javaInformations.MaxBy(x => x.Value.Version).Key;

        return sameMajorJava.MaxBy(x => x.Value.Version).Key;
    }
}
