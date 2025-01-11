using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Launch;
using System;

namespace Natsurainko.FluentLauncher.XamlHelpers.Converters;

internal class LoggerItemConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not GameLoggerOutput output)
            return null;

        var richTextBlock = new RichTextBlock();
        richTextBlock.FontWeight = FontWeights.SemiBold;

        LoggerColorLightLanguage.Formatter.FormatRichTextBlock(output.FullData, new LoggerColorLightLanguage(), richTextBlock);
        return richTextBlock;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
