using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Authentication;
using System;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.Xaml.Converters;

internal partial class LanguageCodeToLanguageInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string languageCode)
            throw new ArgumentException("Value must be a string language code.");

        return LocalizedStrings.SupportedLanguages.First(x => x.LanguageCode == languageCode);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not LanguageInfo info)
            throw new ArgumentException("Value must be a LanguageInfo object.");

        return LocalizedStrings.SupportedLanguages.First(x => x.LanguageCode == info.LanguageCode).LanguageCode;
    }
}
