using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using Nrk.FluentCore.GameManagement.Installer;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public partial class InstanceDescriptionConverter : IValueConverter
{
    public bool EnableShowModLoaderType { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MinecraftInstance game)
        {
            var strings = new List<string>
            {
                game.Version.VersionId,
                ResourceUtils.GetValue("Converters", "_" + game.Version.Type switch
                {
                    MinecraftVersionType.Release => "Release",
                    MinecraftVersionType.Snapshot => "Snapshot",
                    MinecraftVersionType.OldBeta => "Old Beta",
                    MinecraftVersionType.OldAlpha => "Old Alpha",
                    _ => "Unknown"
                })
            };

            if (EnableShowModLoaderType)
                strings.AddRange(game.GetModLoaders().Select(x => $"{x.Type} {x.Version}"));

            return strings;
        }

        if (value is VersionManifestItem manifestItem)
            return ResourceUtils.GetValue("Converters", "_" + manifestItem.Type switch
            {
                "release" => "Release",
                "snapshot" => "Snapshot",
                "old_beta" => "Old Beta",
                "old_alpha" => "Old Alpha",
                _ => "Unknown"
            });

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
