using Microsoft.UI.Xaml.Markup;

namespace Natsurainko.FluentLauncher.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(Microsoft.UI.Xaml.Controls.FontIcon))]
public class FontIcon : MarkupExtension
{
    public string Glyph { get; set; }

    protected override object ProvideValue()
    {
        return new Microsoft.UI.Xaml.Controls.FontIcon { Glyph = Glyph };
    }
}