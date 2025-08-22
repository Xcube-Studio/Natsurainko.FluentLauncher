using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Xaml.Interactivity;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Xaml.Behaviors;
using System;

#nullable disable
namespace Natsurainko.FluentLauncher.Xaml.Converters;

internal partial class SuggestionIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Suggestion suggeastion)
            return null;

        if (suggeastion.SuggestionIconType == SuggestionIconType.Glyph)
            return new Microsoft.UI.Xaml.Controls.FontIcon() { FontSize = 18, Glyph = suggeastion.Icon };
        else if (suggeastion.SuggestionIconType == SuggestionIconType.UriIcon)
            return new Image { Source = new BitmapImage(new Uri(suggeastion.Icon, UriKind.RelativeOrAbsolute)) };
        else
        {
            var image = new Image();
            var behavior = new ImageSourceLoadBehavior
            {
                SourcePropertyName = "Source",
                LoadFromInternet = true,
                ImageSourceUrl = suggeastion.Icon
            };

            Interaction.GetBehaviors(image).Add(behavior);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
