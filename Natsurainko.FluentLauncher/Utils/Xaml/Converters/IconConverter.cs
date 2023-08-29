using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Enums;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Converters;

public class IconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GameInfo game)
            return new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", !game.IsVanilla ? "furnace_front" : game.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }), UriKind.RelativeOrAbsolute));


        if (value is VersionManifestItem manifestItem)
            return new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", manifestItem.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }), UriKind.RelativeOrAbsolute));

        
        if (value is ModLoaderType modLoaderType)
            return new BitmapImage(new Uri($"ms-appx:///Assets/Icons/{modLoaderType}Icon.png", UriKind.RelativeOrAbsolute));

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
