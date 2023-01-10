using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.Toolkits.Values;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentCore.Module.Parser;

public class LibraryParser
{
    public List<LibraryJsonEntity> Entities { get; set; }

    public DirectoryInfo Root { get; set; }

    public LibraryParser(List<LibraryJsonEntity> entities, DirectoryInfo root)
    {
        this.Entities = entities;
        this.Root = root;
    }

    public IEnumerable<LibraryResource> GetLibraries()
    {
        foreach (var libraryJsonEntity in Entities)
        {
            var libraryResource = new LibraryResource
            {
                CheckSum = (libraryJsonEntity.Downloads?.Artifact?.Sha1) ?? string.Empty,
                Size = (libraryJsonEntity.Downloads?.Artifact?.Size == null) ? 0 : (int)libraryJsonEntity.Downloads?.Artifact?.Size,
                Url = ((libraryJsonEntity.Downloads?.Artifact?.Url) ?? string.Empty) + libraryJsonEntity.Url,
                Name = libraryJsonEntity.Name,
                Root = this.Root,
                IsEnable = true
            };

            if (libraryJsonEntity.Rules != null)
                libraryResource.IsEnable = GetAblility(libraryJsonEntity, EnvironmentInfo.GetPlatformName());

            if (libraryJsonEntity.Natives != null)
            {
                libraryResource.IsNatives = true;

                if (!libraryJsonEntity.Natives.ContainsKey(EnvironmentInfo.GetPlatformName()))
                    libraryResource.IsEnable = false;

                if (libraryResource.IsEnable)
                {
                    libraryResource.Name += $":{GetNativeName(libraryJsonEntity)}";

                    var file = libraryJsonEntity.Downloads.Classifiers[libraryJsonEntity.Natives[EnvironmentInfo.GetPlatformName()].Replace("${arch}", EnvironmentInfo.Arch)];

                    libraryResource.CheckSum = file.Sha1;
                    libraryResource.Size = file.Size;
                    libraryResource.Url = file.Url;
                }
            }

            yield return libraryResource;
        }
    }

    private string GetNativeName(LibraryJsonEntity libraryJsonEntity) => libraryJsonEntity.Natives[EnvironmentInfo.GetPlatformName()].Replace("${arch}", EnvironmentInfo.Arch);

    private bool GetAblility(LibraryJsonEntity libraryJsonEntity, string platform)
    {
        bool windows, linux, osx;
        windows = linux = osx = false;

        foreach (var item in libraryJsonEntity.Rules)
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

        return platform switch
        {
            "windows" => windows,
            "linux" => linux,
            "osx" => osx,
            _ => false,
        };
    }
}
