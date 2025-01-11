using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Experimental.Saves;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Collections.Generic;

#nullable disable
namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public partial class SaveInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SaveInfo saveInfo)
            return null;

        var tags = new List<string>();
        tags.Add(ResourceUtils.GetValue("Converters", $"_SaveGameType_{saveInfo.GameType}"));

        if (saveInfo.AllowCommands)
            tags.Add(ResourceUtils.GetValue("Converters", "_SaveCommands"));

        tags.Add(ResourceUtils.GetValue("Converters", "_SaveVersion").Replace("${version}", saveInfo.Version));

        tags.Add(ResourceUtils.GetValue("Converters", "_SaveSeed").Replace("${seed}", saveInfo.Seed.ToString()));
        return string.Join(", ", tags);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
