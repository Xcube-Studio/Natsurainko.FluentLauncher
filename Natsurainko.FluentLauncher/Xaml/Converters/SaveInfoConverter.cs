using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Saves;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Xaml.Converters;

public partial class SaveInfoConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SaveInfo saveInfo)
            return null;

        List<string> tags = [ LocalizedStrings.GetString($"Converters__SaveGameType_{saveInfo.GameType}") ];

        if (saveInfo.AllowCommands)
            tags.Add(LocalizedStrings.Converters__SaveCommands);

        tags.Add(LocalizedStrings.Converters__SaveVersion.Replace("${version}", saveInfo.Version));
        tags.Add(LocalizedStrings.Converters__SaveSeed.Replace("${seed}", saveInfo.Seed.ToString()));

        return string.Join(", ", tags);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
