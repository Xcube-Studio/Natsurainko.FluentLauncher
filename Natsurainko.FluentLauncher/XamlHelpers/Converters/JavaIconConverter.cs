using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

public partial class JavaIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!File.Exists(value.ToString()))
            return new Microsoft.UI.Xaml.Controls.FontIcon() { Glyph = "\ue711" };

        var storageFile = StorageFile.GetFileFromPathAsync(value.ToString()).GetAwaiter().GetResult();
        using var thumbnail = storageFile.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem, 64, Windows.Storage.FileProperties.ThumbnailOptions.ResizeThumbnail).GetAwaiter().GetResult();

        var bitmapImage = new BitmapImage();

        _ = bitmapImage.SetSourceAsync(thumbnail);


        return new ImageIcon() { Source = bitmapImage };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
