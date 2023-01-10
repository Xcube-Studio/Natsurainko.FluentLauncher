using Natsurainko.FluentLauncher.Models;
using Natsurainko.Toolkits.Values;
using System.IO;
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
}
