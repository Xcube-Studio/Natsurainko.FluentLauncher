using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Natsurainko.FluentLauncher.Xaml.Converters;

using FontIcon = Microsoft.UI.Xaml.Controls.FontIcon;

public partial class JavaIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string path ||
            !FileInfoExtensions.TryParse(path, out var fileInfo) || !fileInfo.Exists)
            return new FontIcon() { Glyph = "\ue711" };

        if (fileInfo.LinkTarget is not null)
        {
            if (!File.Exists(fileInfo.LinkTarget))
                return new FontIcon() { Glyph = "\ue711" };

            path = fileInfo.LinkTarget;
        }

        var storageFile = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
        using var thumbnail = storageFile.GetScaledImageAsThumbnailAsync(
            ThumbnailMode.SingleItem, 64, ThumbnailOptions.ResizeThumbnail).GetAwaiter().GetResult();

        BitmapImage bitmapImage = new();
        bitmapImage.SetSourceAsync(thumbnail).AsTask().Forget();

        return new ImageIcon() { Source = bitmapImage };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
