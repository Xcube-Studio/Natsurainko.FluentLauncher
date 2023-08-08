using Microsoft.UI.Xaml.Data;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

public class GameCoreTagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GameInfo game)
        {
            var strings = new List<string>
            {
                ResourceUtils.GetValue("Converters", "_" + game.Type switch
                {
                    "release" => "Release",
                    "snapshot" => "Snapshot",
                    "old_beta" => "Old Beta",
                    "old_alpha" => "Old Alpha",
                    _ => "Unknown"
                })
            };

            if (game.IsInheritedFrom)
                strings.Add(ResourceUtils.GetValue("Converters", "_Inherited From"));

            strings.Add(game.AbsoluteVersion);

            return string.Join(" ", strings);
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
