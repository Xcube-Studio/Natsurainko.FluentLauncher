using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentCore.Model.Launch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Converters;

public class GameCoreTagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GameCore core)
        {
            var strings = new List<string>
            {
                core.Type switch
                {
                    "release" => "Release",
                    "snapshot" => "Snapshot",
                    "old_beta" => "Old Beta",
                    "old_alpha" => "Old Alpha",
                    _ => "Unknown"
                }
            };

            if (!string.IsNullOrEmpty(core.InheritsFrom))
                strings.Add("Inherited From");

            strings.Add(core.Source);

            return string.Join(" ", strings);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
