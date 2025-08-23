using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.Xaml.Converters;

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
                game.Version.Type switch
                {
                    MinecraftVersionType.Release => LocalizedStrings.Converters__Release,
                    MinecraftVersionType.Snapshot => LocalizedStrings.Converters__Snapshot,
                    MinecraftVersionType.OldBeta => LocalizedStrings.Converters__Old_Beta,
                    MinecraftVersionType.OldAlpha => LocalizedStrings.Converters__Old_Alpha,
                    _ => LocalizedStrings.Converters__Unknown
                }
            };

            if (EnableShowModLoaderType)
                strings.AddRange(game.GetModLoaders().Select(x => $"{x.Type} {x.Version}"));

            return strings;
        }

        if (value is VersionManifestItem manifestItem)
            return manifestItem.Type switch
            {
                "release" => LocalizedStrings.Converters__Release,
                "snapshot" => LocalizedStrings.Converters__Snapshot,
                "old_beta" => LocalizedStrings.Converters__Old_Beta,
                "old_alpha" => LocalizedStrings.Converters__Old_Alpha,
                _ => LocalizedStrings.Converters__Unknown
            };

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
