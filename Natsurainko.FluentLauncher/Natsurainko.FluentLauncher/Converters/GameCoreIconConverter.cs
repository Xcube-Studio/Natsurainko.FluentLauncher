using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Components.FluentCore;
using System;

namespace Natsurainko.FluentLauncher.Converters;

public class GameCoreIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GameCore core)
            return new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !core.IsVanilla ? "furnace_front" : core.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }), UriKind.RelativeOrAbsolute));

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
